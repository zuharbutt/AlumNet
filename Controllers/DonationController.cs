using AlumniManagementSystem.Common;
using AlumniManagementSystem.DTOs.Donation;
using AlumniManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AlumniManagementSystem.Controllers;

[ApiController]
[Route("api/donations")]
[Authorize]
public class DonationController(DonationService donationService) : ControllerBase
{
    // POST /api/donations — Alumni only (FR-701)
    [HttpPost]
    [Authorize(Roles = "Alumni")]
    public async Task<IActionResult> MakeDonation([FromBody] MakeDonationDto dto)
    {
        var userId     = GetUserId();
        var donationId = await donationService.MakeDonationAsync(userId, dto);
        return Ok(ApiResponse<object>.Ok(new { donationId }, "Donation successful."));
    }

    // GET /api/donations/mine — Alumni only (FR-707)
    [HttpGet("mine")]
    [Authorize(Roles = "Alumni")]
    public async Task<IActionResult> GetMyDonations(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        var result = await donationService.GetMyDonationsAsync(userId, page, pageSize);
        return Ok(ApiResponse<DonationListResultDto>.Ok(result));
    }

    // ─────────────────────────────────────────────────────────────────────────
    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token."));
}
