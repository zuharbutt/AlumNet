using AlumniManagementSystem.Common;
using AlumniManagementSystem.DTOs.Alumni;
using AlumniManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AlumniManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlumniController(AlumniService alumniService, MentorshipService mentorshipService) : ControllerBase
{
    // GET /api/alumni/me  — own profile (Alumni only)
    [HttpGet("me")]
    [Authorize(Roles = "Alumni")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetUserId();
        var result = await alumniService.GetMyProfileAsync(userId);
        return Ok(ApiResponse<AlumniProfileDto>.Ok(result));
    }

    // PUT /api/alumni/me  — update own profile (Alumni only)
    [HttpPut("me")]
    [Authorize(Roles = "Alumni")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateAlumniProfileDto dto)
    {
        var userId = GetUserId();
        var result = await alumniService.UpdateMyProfileAsync(userId, dto);
        return Ok(ApiResponse<AlumniProfileDto>.Ok(result, "Profile updated successfully."));
    }

    // GET /api/alumni  — directory (Student + Admin only, FR-301, FR-305)
    [HttpGet]
    [Authorize(Roles = "Student,Admin")]
    public async Task<IActionResult> GetDirectory(
        [FromQuery] int?    department = null,
        [FromQuery] int?    yearFrom   = null,
        [FromQuery] int?    yearTo     = null,
        [FromQuery] string? industry   = null,
        [FromQuery] string? location   = null,
        [FromQuery] string? keyword    = null,
        [FromQuery] int     page       = 1,
        [FromQuery] int     pageSize   = 10)
    {
        var result = await alumniService.GetDirectoryAsync(
            department, yearFrom, yearTo, industry, location, keyword, page, pageSize);

        return Ok(ApiResponse<AlumniDirectoryResultDto>.Ok(result));
    }

    // GET /api/alumni/{id}  — single public profile (all authenticated roles)
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var requestingUserId = GetUserId();
        var result = await alumniService.GetByIdAsync(id, requestingUserId);
        return Ok(ApiResponse<AlumniDetailDto>.Ok(result));
    }

    // GET /api/alumni/dashboard — dashboard stats (Alumni only)
    [HttpGet("dashboard")]
    [Authorize(Roles = "Alumni")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = GetUserId();
        var (pending, accepted) = await mentorshipService.GetAlumniStatsAsync(userId);
        return Ok(ApiResponse<object>.Ok(new
        {
            pendingRequests = pending,
            activeMentees   = accepted,
            upcomingEvents  = 0,
            totalDonated    = 0,
        }));
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User identity not found."));
}
