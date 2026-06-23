using AlumniManagementSystem.Common;
using AlumniManagementSystem.DTOs.Event;
using AlumniManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AlumniManagementSystem.Controllers;

[ApiController]
[Route("api/events")]
[Authorize]
public class EventController(EventService eventService) : ControllerBase
{
    // GET /api/events — all authenticated users (FR-501 list)
    [HttpGet]
    public async Task<IActionResult> GetEvents(
        [FromQuery] string?   status     = null,
        [FromQuery] string?   category   = null,
        [FromQuery] DateTime? fromDate   = null,
        [FromQuery] DateTime? toDate     = null,
        [FromQuery] bool      includeAll = false,
        [FromQuery] int       page       = 1,
        [FromQuery] int       pageSize   = 10)
    {
        var userId = GetUserIdOrNull();
        var result = await eventService.GetEventsAsync(
            status, category, fromDate, toDate, includeAll, page, pageSize, userId);
        return Ok(ApiResponse<EventListResultDto>.Ok(result));
    }

    // GET /api/events/{id} — single event, all authenticated users
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEvent(int id)
    {
        var userId = GetUserIdOrNull();
        var ev     = await eventService.GetEventByIdAsync(id, userId);
        return Ok(ApiResponse<EventResponseDto>.Ok(ev));
    }

    // POST /api/events — Admin only (FR-501, FR-502–506)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
    {
        var adminId = GetUserId();
        var eventId = await eventService.CreateEventAsync(adminId, dto);
        return Ok(ApiResponse<object>.Ok(new { eventId }, "Event created successfully."));
    }

    // PUT /api/events/{id} — Admin only, no registrations (FR-503)
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto dto)
    {
        var adminId = GetUserId();
        await eventService.UpdateEventAsync(adminId, id, dto);
        return Ok(ApiResponse.Ok("Event updated successfully."));
    }

    // PUT /api/events/{id}/cancel — Admin only (FR-506)
    [HttpPut("{id:int}/cancel")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CancelEvent(int id)
    {
        var adminId = GetUserId();
        await eventService.CancelEventAsync(adminId, id);
        return Ok(ApiResponse.Ok("Event cancelled successfully."));
    }

    // DELETE /api/events/{id} — Admin only (FR-505)
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var adminId = GetUserId();
        await eventService.DeleteEventAsync(adminId, id);
        return Ok(ApiResponse.Ok("Event deleted successfully."));
    }

    // POST /api/events/{eventId}/register — all authenticated users (FR-601)
    [HttpPost("{eventId:int}/register")]
    public async Task<IActionResult> Register(int eventId)
    {
        var userId = GetUserId();
        var result = await eventService.RegisterForEventAsync(userId, eventId);
        return Ok(ApiResponse<object>.Ok(new
        {
            ticketId   = result.TicketId,
            ticketCode = result.TicketCode,
            price      = result.Price,
        }, "Registration successful. Ticket issued."));
    }

    // GET /api/events/{id}/attendees — Admin only
    [HttpGet("{id:int}/attendees")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAttendees(int id)
    {
        var attendees = await eventService.GetAttendeesAsync(id);
        return Ok(ApiResponse<List<AttendeeDto>>.Ok(attendees));
    }

    // PUT /api/events/{id}/attendance — Admin only (TC-TKT-010)
    [HttpPut("{id:int}/attendance")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkAttendance(int id, [FromBody] List<int> attendedRegistrationIds)
    {
        await eventService.MarkAttendanceAsync(id, attendedRegistrationIds);
        return Ok(ApiResponse.Ok("Attendance marked."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token."));

    private int? GetUserIdOrNull()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim) : null;
    }
}
