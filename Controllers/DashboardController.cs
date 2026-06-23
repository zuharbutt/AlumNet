using AlumniManagementSystem.Common;
using AlumniManagementSystem.DTOs.Dashboard;
using AlumniManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AlumniManagementSystem.Controllers;

[ApiController]
[Route("api/dashboards")]
[Authorize]
public class DashboardController(DashboardService dashboardService) : ControllerBase
{
    // GET /api/dashboards/student — Student only (FR-804, TC-DASH-002)
    [HttpGet("student")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetStudentDashboard()
    {
        var userId = GetUserId();
        var data   = await dashboardService.GetStudentDashboardAsync(userId);
        return Ok(ApiResponse<StudentDashboardDto>.Ok(data));
    }

    // GET /api/dashboards/alumni — Alumni only (FR-803, TC-DASH-003)
    [HttpGet("alumni")]
    [Authorize(Roles = "Alumni")]
    public async Task<IActionResult> GetAlumniDashboard()
    {
        var userId = GetUserId();
        var data   = await dashboardService.GetAlumniDashboardAsync(userId);
        return Ok(ApiResponse<AlumniDashboardDto>.Ok(data));
    }

    // GET /api/dashboards/admin — Admin only (FR-802, TC-DASH-001)
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminDashboard()
    {
        var data = await dashboardService.GetAdminDashboardAsync();
        return Ok(ApiResponse<AdminDashboardDto>.Ok(data));
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token."));
}
