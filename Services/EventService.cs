using AlumniManagementSystem.Data;
using AlumniManagementSystem.DTOs.Event;
using AlumniManagementSystem.DTOs.Ticket;
using AlumniManagementSystem.Entities;
using AlumniManagementSystem.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AlumniManagementSystem.Services;

public class EventService(AppDbContext db)
{
    private static readonly string[] ValidCategories =
        ["Reunion", "Seminar", "Workshop", "Networking", "Conference", "Other"];

    // ── POST /api/events (Admin only) ─────────────────────────────────────────
    public async Task<int> CreateEventAsync(int adminUserId, CreateEventDto dto)
    {
        ValidateCategory(dto.Category);

        if (dto.EventDate < DateTime.UtcNow.AddHours(24))
            throw new ArgumentException("Event date must be at least 24 hours in the future.");

        if (dto.Capacity < 1)
            throw new ArgumentException("Capacity must be at least 1.");

        if (dto.TicketPrice < 0)
            throw new ArgumentException("Price cannot be negative.");

        var ev = new Event
        {
            Title           = dto.Title,
            Description     = dto.Description,
            Category        = dto.Category,
            EventDate       = dto.EventDate,
            Location        = dto.Location,
            Capacity        = dto.Capacity,
            TicketPrice     = dto.TicketPrice,
            Status          = EventStatus.Scheduled,
            CreatedByUserId = adminUserId,
            CreatedAt       = DateTime.UtcNow,
        };

        db.Events.Add(ev);
        await db.SaveChangesAsync();
        return ev.EventId;
    }

    // ── PUT /api/events/{id} (Admin only, no registrations) ──────────────────
    public async Task UpdateEventAsync(int adminUserId, int eventId, UpdateEventDto dto)
    {
        ValidateCategory(dto.Category);

        var ev = await db.Events.FindAsync(eventId)
            ?? throw new KeyNotFoundException("Event not found.");

        if (ev.Status == EventStatus.Cancelled)
            throw new InvalidOperationException("Cannot edit a cancelled event.");

        var hasRegistrations = await db.EventRegistrations
            .AnyAsync(r => r.EventId == eventId && r.CancelledAt == null);

        if (hasRegistrations)
            throw new InvalidOperationException("Cannot edit event with existing registrations.");

        if (dto.EventDate < DateTime.UtcNow.AddHours(24))
            throw new ArgumentException("Event date must be at least 24 hours in the future.");

        if (dto.Capacity < 1)
            throw new ArgumentException("Capacity must be at least 1.");

        if (dto.TicketPrice < 0)
            throw new ArgumentException("Price cannot be negative.");

        ev.Title       = dto.Title;
        ev.Description = dto.Description;
        ev.Category    = dto.Category;
        ev.EventDate   = dto.EventDate;
        ev.Location    = dto.Location;
        ev.Capacity    = dto.Capacity;
        ev.TicketPrice = dto.TicketPrice;
        ev.UpdatedAt   = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }

    // ── PUT /api/events/{id}/cancel (Admin only) ──────────────────────────────
    public async Task CancelEventAsync(int adminUserId, int eventId)
    {
        var ev = await db.Events.FindAsync(eventId)
            ?? throw new KeyNotFoundException("Event not found.");

        if (ev.Status == EventStatus.Cancelled)
            throw new InvalidOperationException("Event is already cancelled.");

        ev.Status    = EventStatus.Cancelled;
        ev.UpdatedAt = DateTime.UtcNow;
        // DB trigger tr_Events_AfterCancel handles cascading cancellation + refunds
        await db.SaveChangesAsync();
    }

    // ── DELETE /api/events/{id} (Admin only, no registrations) ───────────────
    public async Task DeleteEventAsync(int adminUserId, int eventId)
    {
        var ev = await db.Events.FindAsync(eventId)
            ?? throw new KeyNotFoundException("Event not found.");

        var hasRegistrations = await db.EventRegistrations
            .AnyAsync(r => r.EventId == eventId);

        if (hasRegistrations)
            throw new InvalidOperationException("Cannot delete event with registrations. Cancel it instead.");

        db.Events.Remove(ev);
        await db.SaveChangesAsync();
    }

    // ── GET /api/events ───────────────────────────────────────────────────────
    public async Task<EventListResultDto> GetEventsAsync(
        string? status, string? category, DateTime? fromDate, DateTime? toDate,
        bool includeAll, int page, int pageSize, int? currentUserId = null)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        page     = Math.Max(page, 1);

        var query = db.Events
            .Include(e => e.CreatedBy)
            .Include(e => e.Registrations)
            .AsQueryable();

        if (!includeAll)
        {
            // Default: only scheduled events
            query = string.IsNullOrWhiteSpace(status)
                ? query.Where(e => e.Status == EventStatus.Scheduled)
                : query.Where(e => e.Status == status);
        }
        else if (!string.IsNullOrWhiteSpace(status))
        {
            if (string.Equals(status, EventStatus.Completed, StringComparison.OrdinalIgnoreCase))
            {
                var now = DateTime.UtcNow;
                query = query.Where(e =>
                    e.Status == EventStatus.Completed ||
                    (e.Status == EventStatus.Scheduled && e.EventDate < now));
            }
            else
            {
                query = query.Where(e => e.Status == status);
            }
        }

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(e => e.Category == category);

        if (fromDate.HasValue)
            query = query.Where(e => e.EventDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(e => e.EventDate <= toDate.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(e => e.EventDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(e => MapToDto(e, currentUserId)).ToList();

        return new EventListResultDto
        {
            Items      = dtos,
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
        };
    }

    // ── GET /api/events/{id} ──────────────────────────────────────────────────
    public async Task<EventResponseDto> GetEventByIdAsync(int eventId, int? currentUserId = null)
    {
        var ev = await db.Events
            .Include(e => e.CreatedBy)
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.EventId == eventId)
            ?? throw new KeyNotFoundException("Event not found.");

        return MapToDto(ev, currentUserId);
    }

    // ── POST /api/events/{eventId}/register ───────────────────────────────────
    public async Task<RegisterForEventResultDto> RegisterForEventAsync(int userId, int eventId)
    {
        var ticketCodeParam = new SqlParameter
        {
            ParameterName = "@TicketCode",
            SqlDbType     = System.Data.SqlDbType.NVarChar,
            Size          = 50,
            Direction     = System.Data.ParameterDirection.Output
        };

        var errorMsgParam = new SqlParameter
        {
            ParameterName = "@ErrorMsg",
            SqlDbType     = System.Data.SqlDbType.NVarChar,
            Size          = 500,
            Direction     = System.Data.ParameterDirection.Output
        };

        await db.Database.ExecuteSqlRawAsync(
            "EXEC sp_RegisterForEvent @EventId, @UserId, @TicketCode OUTPUT, @ErrorMsg OUTPUT",
            new SqlParameter("@EventId", eventId),
            new SqlParameter("@UserId",  userId),
            ticketCodeParam,
            errorMsgParam);

        var errorMsg = errorMsgParam.Value as string;
        if (!string.IsNullOrWhiteSpace(errorMsg))
            throw new InvalidOperationException(errorMsg);

        var ticketCode = ticketCodeParam.Value?.ToString() ?? string.Empty;

        var ticket = await db.Tickets
            .Include(t => t.Registration)
            .FirstOrDefaultAsync(t => t.TicketCode == ticketCode)
            ?? throw new Exception("Ticket creation failed.");

        return new RegisterForEventResultDto
        {
            TicketId   = ticket.TicketId,
            TicketCode = ticket.TicketCode,
            Price      = ticket.Price,
        };
    }

    // ── GET /api/tickets/mine ─────────────────────────────────────────────────
    public async Task<List<TicketItemDto>> GetUserTicketsAsync(int userId)
    {
        var tickets = await db.Tickets
            .Include(t => t.Registration)
                .ThenInclude(r => r!.Event)
            .Where(t => t.Registration!.UserId == userId)
            .OrderByDescending(t => t.Registration!.Event!.EventDate)
            .ToListAsync();

        return tickets.Select(t => new TicketItemDto
        {
            TicketId       = t.TicketId,
            RegistrationId = t.RegistrationId,
            TicketCode     = t.TicketCode,
            Price          = t.Price,
            PaymentStatus  = t.PaymentStatus,
            IssuedAt       = t.IssuedAt,
            RefundedAt     = t.RefundedAt,
            CancelledAt    = t.Registration!.CancelledAt,
            EventId        = t.Registration.Event!.EventId,
            EventTitle     = t.Registration.Event.Title,
            EventDate      = t.Registration.Event.EventDate,
            EventLocation  = t.Registration.Event.Location,
            EventCategory  = t.Registration.Event.Category,
            EventStatus    = t.Registration.Event.Status,
        }).ToList();
    }

    // ── DELETE /api/tickets/{ticketId} (cancel registration) ─────────────────
    public async Task CancelRegistrationAsync(int userId, int ticketId)
    {
        var ticket = await db.Tickets
            .Include(t => t.Registration)
                .ThenInclude(r => r!.Event)
            .FirstOrDefaultAsync(t => t.TicketId == ticketId)
            ?? throw new KeyNotFoundException("Ticket not found.");

        if (ticket.Registration!.UserId != userId)
            throw new UnauthorizedAccessException("You can only cancel your own registrations.");

        if (ticket.Registration.CancelledAt != null)
            throw new InvalidOperationException("Registration is already cancelled.");

        var eventDate = ticket.Registration.Event!.EventDate;

        if (eventDate <= DateTime.UtcNow)
            throw new InvalidOperationException("Cannot cancel registration for a past event.");

        // FR-606: only if > 24h before event
        if (eventDate <= DateTime.UtcNow.AddHours(24))
            throw new InvalidOperationException("Cannot cancel registration within 24 hours of the event.");

        // Mark registration as cancelled
        ticket.Registration.CancelledAt = DateTime.UtcNow;

        // FR-607: full refund for cancellation >= 24h before event
        ticket.PaymentStatus = PaymentStatus.Refunded;
        ticket.RefundedAt    = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }

    // ── GET /api/events/{id}/attendees (Admin only) ───────────────────────────
    public async Task<List<AttendeeDto>> GetAttendeesAsync(int eventId)
    {
        var registrations = await db.EventRegistrations
            .Include(r => r.User)
            .Include(r => r.Ticket)
            .Where(r => r.EventId == eventId && r.CancelledAt == null)
            .OrderBy(r => r.RegisteredAt)
            .ToListAsync();

        return registrations.Select(r => new AttendeeDto
        {
            RegistrationId = r.RegistrationId,
            UserId         = r.UserId,
            FullName       = r.User!.FullName,
            Email          = r.User.Email,
            TicketCode     = r.Ticket?.TicketCode ?? string.Empty,
            Attended       = r.Attended,
            RegisteredAt   = r.RegisteredAt,
        }).ToList();
    }

    // ── PUT /api/events/{id}/attendance (Admin only) ──────────────────────────
    public async Task MarkAttendanceAsync(int eventId, List<int> attendedRegistrationIds)
    {
        var registrations = await db.EventRegistrations
            .Where(r => r.EventId == eventId && r.CancelledAt == null)
            .ToListAsync();

        foreach (var reg in registrations)
            reg.Attended = attendedRegistrationIds.Contains(reg.RegistrationId);

        await db.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    private static EventResponseDto MapToDto(Event e, int? currentUserId)
    {
        var activeCount = e.Registrations.Count(r => r.CancelledAt == null);
        var isRegistered = currentUserId.HasValue
            ? e.Registrations.Any(r => r.UserId == currentUserId.Value && r.CancelledAt == null)
            : (bool?)null;

        return new EventResponseDto
        {
            EventId         = e.EventId,
            Title           = e.Title,
            Description     = e.Description,
            Category        = e.Category,
            EventDate       = e.EventDate,
            Location        = e.Location,
            Capacity        = e.Capacity,
            TicketPrice     = e.TicketPrice,
            Status          = e.Status,
            RegisteredCount = activeCount,
            SeatsRemaining  = Math.Max(0, e.Capacity - activeCount),
            IsFull          = activeCount >= e.Capacity,
            IsRegistered    = isRegistered,
            CreatedAt       = e.CreatedAt,
            UpdatedAt       = e.UpdatedAt,
            CreatedByName   = e.CreatedBy?.FullName ?? string.Empty,
        };
    }

    private static void ValidateCategory(string category)
    {
        if (!ValidCategories.Contains(category))
            throw new ArgumentException($"Category must be one of: {string.Join(", ", ValidCategories)}.");
    }
}

// ── Attendee DTO (used only by EventService / EventController) ────────────────
public class AttendeeDto
{
    public int      RegistrationId { get; set; }
    public int      UserId         { get; set; }
    public string   FullName       { get; set; } = string.Empty;
    public string   Email          { get; set; } = string.Empty;
    public string   TicketCode     { get; set; } = string.Empty;
    public bool     Attended       { get; set; }
    public DateTime RegisteredAt   { get; set; }
}
