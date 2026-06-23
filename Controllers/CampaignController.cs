using AlumniManagementSystem.Common;
using AlumniManagementSystem.DTOs.Donation;
using AlumniManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AlumniManagementSystem.Controllers;

[ApiController]
[Route("api/campaigns")]
[Authorize]
public class CampaignController(CampaignService campaignService) : ControllerBase
{
    // POST /api/campaigns — Admin only (FR-702)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCampaign([FromBody] CreateCampaignDto dto)
    {
        var adminId    = GetUserId();
        var campaignId = await campaignService.CreateCampaignAsync(adminId, dto);
        return Ok(ApiResponse<object>.Ok(new { campaignId }, "Campaign created successfully."));
    }

    // PUT /api/campaigns/{id} — Admin only, Active campaigns only (FR-702)
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCampaign(int id, [FromBody] UpdateCampaignDto dto)
    {
        var adminId = GetUserId();
        await campaignService.UpdateCampaignAsync(adminId, id, dto);
        return Ok(ApiResponse.Ok("Campaign updated successfully."));
    }

    // PUT /api/campaigns/{id}/close — Admin only
    [HttpPut("{id:int}/close")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CloseCampaign(int id)
    {
        var adminId = GetUserId();
        await campaignService.CloseCampaignAsync(adminId, id);
        return Ok(ApiResponse.Ok("Campaign closed successfully."));
    }

    // GET /api/campaigns — all auth, paginated, optional ?status= filter
    [HttpGet]
    public async Task<IActionResult> GetCampaigns(
        [FromQuery] string? status   = null,
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 10)
    {
        var result = await campaignService.GetCampaignsAsync(status, page, pageSize);
        return Ok(ApiResponse<CampaignListResultDto>.Ok(result));
    }

    // GET /api/campaigns/{id} — all auth
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCampaign(int id)
    {
        var campaign = await campaignService.GetCampaignByIdAsync(id);
        return Ok(ApiResponse<CampaignResponseDto>.Ok(campaign));
    }

    // ─────────────────────────────────────────────────────────────────────────
    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token."));
}
