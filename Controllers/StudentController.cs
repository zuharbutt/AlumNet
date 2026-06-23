using AlumniManagementSystem.Common;
using AlumniManagementSystem.DTOs.Student;
using AlumniManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AlumniManagementSystem.Controllers;

[ApiController]
[Route("api/students")]
[Authorize]
public class StudentController(StudentService studentService, MentorshipService mentorshipService) : ControllerBase
{
    // GET /api/students/me  — own profile (Student only)
    [HttpGet("me")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetUserId();
        var result = await studentService.GetMyProfileAsync(userId);
        return Ok(ApiResponse<StudentProfileDto>.Ok(result));
    }

    // PUT /api/students/me  — update own profile (Student only)
    [HttpPut("me")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateStudentProfileDto dto)
    {
        var userId = GetUserId();
        var result = await studentService.UpdateMyProfileAsync(userId, dto);
        return Ok(ApiResponse<StudentProfileDto>.Ok(result, "Profile updated successfully."));
    }

    // GET /api/students/dashboard  — dashboard stats (Student only)
    [HttpGet("dashboard")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = GetUserId();
        var (pending, accepted) = await mentorshipService.GetStudentStatsAsync(userId);
        return Ok(ApiResponse<object>.Ok(new
        {
            pendingRequests = pending,
            acceptedMentors = accepted,
            upcomingEvents  = 0,
            myTickets       = 0,
        }));
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User identity not found."));
}
