using AlumniManagementSystem.Common;
using AlumniManagementSystem.DTOs.Mentorship;
using AlumniManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AlumniManagementSystem.Controllers;

[ApiController]
[Route("api/mentorship")]
[Authorize]
public class MentorshipController(MentorshipService mentorshipService) : ControllerBase
{
    // POST /api/mentorship/request — Student sends request to alumnus (FR-401, FR-402, FR-403)
    [HttpPost("request")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> SendRequest([FromBody] SendMentorshipRequestDto dto)
    {
        var studentId = GetUserId();
        var requestId = await mentorshipService.SendRequestAsync(studentId, dto);
        return Ok(ApiResponse<object>.Ok(new { requestId }, "Mentorship request sent successfully."));
    }

    // PUT /api/mentorship/{requestId}/respond — Alumni responds to a request (FR-404, FR-406)
    [HttpPut("{requestId:int}/respond")]
    [Authorize(Roles = "Alumni")]
    public async Task<IActionResult> Respond(int requestId, [FromBody] RespondToRequestDto dto)
    {
        var alumniId = GetUserId();
        await mentorshipService.RespondAsync(alumniId, requestId, dto);
        return Ok(ApiResponse.Ok($"Request {dto.Status.ToLower()} successfully."));
    }

    // GET /api/mentorship/sent — Student views own sent requests (FR-405)
    [HttpGet("sent")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetSent(
        [FromQuery] string? status   = null,
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 10)
    {
        var studentId = GetUserId();
        var result    = await mentorshipService.GetSentAsync(studentId, status, page, pageSize);
        return Ok(ApiResponse<MentorshipListResultDto>.Ok(result));
    }

    // GET /api/mentorship/received — Alumni views own inbox (FR-405)
    [HttpGet("received")]
    [Authorize(Roles = "Alumni")]
    public async Task<IActionResult> GetReceived(
        [FromQuery] string? status   = null,
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 10)
    {
        var alumniId = GetUserId();
        var result   = await mentorshipService.GetReceivedAsync(alumniId, status, page, pageSize);
        return Ok(ApiResponse<MentorshipListResultDto>.Ok(result));
    }

    // GET /api/mentorship/status?alumniId=X — Student checks status with a specific alumnus (FR-407)
    [HttpGet("status")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetStatus([FromQuery] int alumniId)
    {
        var studentId = GetUserId();
        var result    = await mentorshipService.GetStatusAsync(studentId, alumniId);
        return Ok(ApiResponse<MentorshipStatusDto>.Ok(result));
    }

    // DELETE /api/mentorship/{requestId} — Student cancels own Pending request (FR-408)
    [HttpDelete("{requestId:int}")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Cancel(int requestId)
    {
        var studentId = GetUserId();
        await mentorshipService.CancelAsync(studentId, requestId);
        return Ok(ApiResponse.Ok("Request cancelled successfully."));
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User identity not found."));
}
