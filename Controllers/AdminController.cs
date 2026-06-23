using AlumniManagementSystem.Common;
using AlumniManagementSystem.DTOs.Dashboard;
using AlumniManagementSystem.DTOs.Donation;
using AlumniManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlumniManagementSystem.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(
    DonationService donationService,
    DashboardService dashboardService) : ControllerBase
{
    // GET /api/admin/users?role=alumni|student — SRS §11.6, FR-802
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string role     = "Alumni",
        [FromQuery] int    page     = 1,
        [FromQuery] int    pageSize = 20)
    {
        if (role != "Alumni" && role != "Student")
            return BadRequest(ApiResponse<object>.Fail("Role must be 'Alumni' or 'Student'."));

        var result = await dashboardService.GetAdminUsersAsync(role, page, pageSize);
        return Ok(ApiResponse<AdminUserListDto>.Ok(result));
    }

    // GET /api/admin/mentorship?status=&page=&pageSize= — SRS §11.6, DDD §6 vw_MentorshipOverview
    [HttpGet("mentorship")]
    public async Task<IActionResult> GetMentorship(
        [FromQuery] string? status   = null,
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 20)
    {
        var result = await dashboardService.GetAdminMentorshipAsync(status, page, pageSize);
        return Ok(ApiResponse<AdminMentorshipListDto>.Ok(result));
    }

    // GET /api/admin/donations — Admin only, all donations (FR-707, TC-DON-009)
    [HttpGet("donations")]
    public async Task<IActionResult> GetAllDonations(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await donationService.GetAllDonationsAsync(page, pageSize);
        return Ok(ApiResponse<DonationListResultDto>.Ok(result));
    }

    // GET /api/admin/reports/mentorship — Admin only (FR-805, TC-DASH-004)
    [HttpGet("reports/mentorship")]
    public async Task<IActionResult> GetMentorshipReport(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate   = null)
    {
        var data = await dashboardService.GetMentorshipReportAsync(fromDate, toDate);
        return Ok(ApiResponse<MentorshipReportDto>.Ok(data));
    }

    // GET /api/admin/reports/events — Admin only (FR-805)
    [HttpGet("reports/events")]
    public async Task<IActionResult> GetEventReport()
    {
        var data = await dashboardService.GetEventReportAsync();
        return Ok(ApiResponse<EventReportDto>.Ok(data));
    }

    // GET /api/admin/reports/donations — Admin only (FR-805)
    [HttpGet("reports/donations")]
    public async Task<IActionResult> GetDonationReport([FromQuery] int? campaignId = null)
    {
        var data = await dashboardService.GetDonationReportAsync(campaignId);
        return Ok(ApiResponse<DonationReportDto>.Ok(data));
    }
}
