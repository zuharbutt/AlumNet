using AlumniManagementSystem.Common;
using AlumniManagementSystem.DTOs.Ticket;
using AlumniManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AlumniManagementSystem.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketController(EventService eventService) : ControllerBase
{
    // GET /api/tickets/mine — all authenticated users (TC-TKT-009, FR-601)
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyTickets()
    {
        var userId  = GetUserId();
        var tickets = await eventService.GetUserTicketsAsync(userId);
        return Ok(ApiResponse<List<TicketItemDto>>.Ok(tickets));
    }

    // DELETE /api/tickets/{id} — cancel registration (FR-606, FR-607)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> CancelRegistration(int id)
    {
        var userId = GetUserId();
        await eventService.CancelRegistrationAsync(userId, id);
        return Ok(ApiResponse.Ok("Registration cancelled and ticket refunded."));
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token."));
}
