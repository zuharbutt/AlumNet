using AlumniManagementSystem.Data;
using AlumniManagementSystem.DTOs.Mentorship;
using AlumniManagementSystem.Entities;
using AlumniManagementSystem.Enums;
using Microsoft.EntityFrameworkCore;

namespace AlumniManagementSystem.Services;

public class MentorshipService(AppDbContext db)
{
    // ── POST /api/mentorship/request (Student only) ───────────────────────────
    public async Task<int> SendRequestAsync(int studentUserId, SendMentorshipRequestDto dto)
    {
        if (dto.Message.Length < 20)
            throw new ArgumentException("Message must be at least 20 characters.");
        if (dto.Message.Length > 500)
            throw new ArgumentException("Message cannot exceed 500 characters.");

        // Alumnus must exist and be Alumni role
        var alumni = await db.Users.FirstOrDefaultAsync(u => u.UserId == dto.AlumniId && u.Role == "Alumni" && u.IsActive)
            ?? throw new ArgumentException("Alumnus not found.");

        // Student cannot send to themselves
        if (studentUserId == dto.AlumniId)
            throw new ArgumentException("You cannot send a mentorship request to yourself.");

        // Prevent duplicate active request
        var existing = await db.MentorshipRequests.AnyAsync(m =>
            m.StudentUserId == studentUserId &&
            m.AlumniUserId  == dto.AlumniId &&
            (m.Status == RequestStatus.Pending || m.Status == RequestStatus.Accepted));

        if (existing)
            throw new InvalidOperationException("Request already exists. You already have a pending or active request with this alumnus.");

        var request = new MentorshipRequest
        {
            StudentUserId = studentUserId,
            AlumniUserId  = dto.AlumniId,
            Message       = dto.Message,
            Status        = RequestStatus.Pending,
            RequestedAt   = DateTime.UtcNow,
        };

        db.MentorshipRequests.Add(request);
        await db.SaveChangesAsync();
        return request.RequestId;
    }

    // ── PUT /api/mentorship/{requestId}/respond (Alumni only) ─────────────────
    public async Task RespondAsync(int alumniUserId, int requestId, RespondToRequestDto dto)
    {
        var allowed = new[] { RequestStatus.Accepted, RequestStatus.Rejected };
        if (!allowed.Contains(dto.Status))
            throw new ArgumentException("Status must be 'Accepted' or 'Rejected'.");

        var request = await db.MentorshipRequests.FindAsync(requestId)
            ?? throw new KeyNotFoundException("Mentorship request not found.");

        // Must be the target alumnus
        if (request.AlumniUserId != alumniUserId)
            throw new UnauthorizedAccessException("You can only respond to your own requests.");

        // Only Pending can be responded to
        if (request.Status != RequestStatus.Pending)
            throw new InvalidOperationException("Status cannot be changed once responded.");

        request.Status      = dto.Status;
        request.RespondedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    // ── GET /api/mentorship/sent (Student only) ───────────────────────────────
    public async Task<MentorshipListResultDto> GetSentAsync(int studentUserId, string? status, int page, int pageSize)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        page     = Math.Max(page, 1);

        var query = db.MentorshipRequests
            .Include(m => m.Alumni)
            .Include(m => m.Student)
            .Where(m => m.StudentUserId == studentUserId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(m => m.Status == status);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(m => m.RequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new MentorshipListResultDto
        {
            TotalCount = total,
            PageNumber = page,
            PageSize   = pageSize,
            Items      = items.Select(m => MapToItemDto(m, showAlumniContact: m.Status == RequestStatus.Accepted)).ToList(),
        };
    }

    // ── GET /api/mentorship/received (Alumni only) ────────────────────────────
    public async Task<MentorshipListResultDto> GetReceivedAsync(int alumniUserId, string? status, int page, int pageSize)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        page     = Math.Max(page, 1);

        var query = db.MentorshipRequests
            .Include(m => m.Student)
            .Include(m => m.Alumni)
            .Where(m => m.AlumniUserId == alumniUserId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(m => m.Status == status);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(m => m.RequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new MentorshipListResultDto
        {
            TotalCount = total,
            PageNumber = page,
            PageSize   = pageSize,
            Items      = items.Select(m => MapToItemDto(m, showAlumniContact: true)).ToList(),
        };
    }

    // ── GET /api/mentorship/status?alumniId=X (Student only) ─────────────────
    public async Task<MentorshipStatusDto> GetStatusAsync(int studentUserId, int alumniId)
    {
        var request = await db.MentorshipRequests
            .Where(m => m.StudentUserId == studentUserId && m.AlumniUserId == alumniId)
            .OrderByDescending(m => m.RequestedAt)
            .FirstOrDefaultAsync();

        if (request == null)
            return new MentorshipStatusDto { HasRequest = false };

        return new MentorshipStatusDto
        {
            HasRequest = true,
            Status     = request.Status,
            RequestId  = request.RequestId,
        };
    }

    // ── DELETE /api/mentorship/{requestId} (Student only) ────────────────────
    public async Task CancelAsync(int studentUserId, int requestId)
    {
        var request = await db.MentorshipRequests.FindAsync(requestId)
            ?? throw new KeyNotFoundException("Mentorship request not found.");

        if (request.StudentUserId != studentUserId)
            throw new UnauthorizedAccessException("You can only cancel your own requests.");

        if (request.Status != RequestStatus.Pending)
            throw new InvalidOperationException("Only Pending requests can be cancelled.");

        request.Status      = RequestStatus.Cancelled;
        request.RespondedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    public async Task<int> CountPendingReceivedAsync(int alumniUserId) =>
        await db.MentorshipRequests.CountAsync(m => m.AlumniUserId == alumniUserId && m.Status == RequestStatus.Pending);

    public async Task<(int pending, int accepted)> GetStudentStatsAsync(int studentUserId)
    {
        var pending  = await db.MentorshipRequests.CountAsync(m => m.StudentUserId == studentUserId && m.Status == RequestStatus.Pending);
        var accepted = await db.MentorshipRequests.CountAsync(m => m.StudentUserId == studentUserId && m.Status == RequestStatus.Accepted);
        return (pending, accepted);
    }

    public async Task<(int pending, int accepted)> GetAlumniStatsAsync(int alumniUserId)
    {
        var pending  = await db.MentorshipRequests.CountAsync(m => m.AlumniUserId == alumniUserId && m.Status == RequestStatus.Pending);
        var accepted = await db.MentorshipRequests.CountAsync(m => m.AlumniUserId == alumniUserId && m.Status == RequestStatus.Accepted);
        return (pending, accepted);
    }

    private static MentorshipRequestItemDto MapToItemDto(MentorshipRequest m, bool showAlumniContact) => new()
    {
        RequestId     = m.RequestId,
        StudentUserId = m.StudentUserId,
        StudentName   = m.Student?.FullName  ?? string.Empty,
        StudentEmail  = m.Student?.Email,
        AlumniUserId  = m.AlumniUserId,
        AlumniName    = m.Alumni?.FullName   ?? string.Empty,
        AlumniEmail   = showAlumniContact ? m.Alumni?.Email : null,
        AlumniPhone   = showAlumniContact ? m.Alumni?.Phone : null,
        Message       = m.Message,
        Status        = m.Status,
        RequestedAt   = m.RequestedAt,
        RespondedAt   = m.RespondedAt,
    };
}
