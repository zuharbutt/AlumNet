using AlumniManagementSystem.Data;
using AlumniManagementSystem.DTOs.Donation;
using AlumniManagementSystem.Entities;
using AlumniManagementSystem.Enums;
using Microsoft.EntityFrameworkCore;

namespace AlumniManagementSystem.Services;

public class CampaignService(AppDbContext db)
{
    private static readonly string[] ValidStatuses = [CampaignStatus.Active, CampaignStatus.Closed, CampaignStatus.Cancelled];

    // ── POST /api/campaigns (Admin only) ─────────────────────────────────────
    public async Task<int> CreateCampaignAsync(int adminId, CreateCampaignDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Title is required.");

        if (string.IsNullOrWhiteSpace(dto.Description))
            throw new ArgumentException("Description is required.");

        if (dto.TargetAmount <= 0)
            throw new ArgumentException("Target amount must be greater than 0.");

        if (dto.EndDate <= DateTime.UtcNow)
            throw new ArgumentException("End date must be in the future.");

        var status = string.IsNullOrWhiteSpace(dto.Status) ? CampaignStatus.Active : dto.Status;
        if (!ValidStatuses.Contains(status))
            throw new ArgumentException($"Status must be one of: {string.Join(", ", ValidStatuses)}.");

        var campaign = new DonationCampaign
        {
            Title           = dto.Title.Trim(),
            Description     = dto.Description.Trim(),
            TargetAmount    = dto.TargetAmount,
            EndDate         = dto.EndDate,
            Status          = status,
            CreatedByUserId = adminId,
            StartDate       = DateTime.UtcNow,
            CreatedAt       = DateTime.UtcNow,
        };

        db.DonationCampaigns.Add(campaign);
        await db.SaveChangesAsync();
        return campaign.CampaignId;
    }

    // ── PUT /api/campaigns/{id} (Admin only, Active campaigns only) ──────────
    public async Task UpdateCampaignAsync(int adminId, int campaignId, UpdateCampaignDto dto)
    {
        var campaign = await db.DonationCampaigns.FindAsync(campaignId)
            ?? throw new KeyNotFoundException("Campaign not found.");

        if (campaign.Status != CampaignStatus.Active)
            throw new InvalidOperationException("Cannot edit closed campaign.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Title is required.");

        if (dto.TargetAmount <= 0)
            throw new ArgumentException("Target amount must be greater than 0.");

        if (dto.EndDate <= DateTime.UtcNow)
            throw new ArgumentException("End date must be in the future.");

        if (!string.IsNullOrWhiteSpace(dto.Status) && !ValidStatuses.Contains(dto.Status))
            throw new ArgumentException($"Status must be one of: {string.Join(", ", ValidStatuses)}.");

        campaign.Title        = dto.Title.Trim();
        campaign.Description  = dto.Description?.Trim() ?? campaign.Description;
        campaign.TargetAmount = dto.TargetAmount;
        campaign.EndDate      = dto.EndDate;
        campaign.Status       = string.IsNullOrWhiteSpace(dto.Status) ? campaign.Status : dto.Status;
        campaign.UpdatedAt    = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }

    // ── PUT /api/campaigns/{id}/close (Admin only) ───────────────────────────
    public async Task CloseCampaignAsync(int adminId, int campaignId)
    {
        var campaign = await db.DonationCampaigns.FindAsync(campaignId)
            ?? throw new KeyNotFoundException("Campaign not found.");

        if (campaign.Status != CampaignStatus.Active)
            throw new InvalidOperationException("Campaign is not active.");

        campaign.Status    = CampaignStatus.Closed;
        campaign.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }

    // ── GET /api/campaigns (all auth, paginated, optional status filter) ──────
    public async Task<CampaignListResultDto> GetCampaignsAsync(
        string? status, int page, int pageSize)
    {
        // Auto-close any Active campaigns whose end date has passed
        var now = DateTime.UtcNow;
        var expired = await db.DonationCampaigns
            .Where(c => c.Status == CampaignStatus.Active && c.EndDate <= now)
            .ToListAsync();
        foreach (var c in expired)
        {
            c.Status    = CampaignStatus.Closed;
            c.UpdatedAt = now;
        }
        if (expired.Count > 0) await db.SaveChangesAsync();

        var query = db.DonationCampaigns.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(c => c.Status == status);

        var total  = await query.CountAsync();
        var offset = (page - 1) * pageSize;

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip(offset)
            .Take(pageSize)
            .Select(c => MapToDto(c))
            .ToListAsync();

        return new CampaignListResultDto
        {
            Items      = items,
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
        };
    }

    // ── GET /api/campaigns/{id} (all auth) ────────────────────────────────────
    public async Task<CampaignResponseDto> GetCampaignByIdAsync(int campaignId)
    {
        var campaign = await db.DonationCampaigns
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CampaignId == campaignId)
            ?? throw new KeyNotFoundException("Campaign not found.");

        return MapToDto(campaign);
    }

    // ─────────────────────────────────────────────────────────────────────────
    private static CampaignResponseDto MapToDto(DonationCampaign c) => new()
    {
        CampaignId      = c.CampaignId,
        Title           = c.Title,
        Description     = c.Description,
        TargetAmount    = c.TargetAmount,
        RaisedAmount    = c.RaisedAmount,
        ProgressPercent = c.TargetAmount > 0
            ? Math.Round(c.RaisedAmount * 100m / c.TargetAmount, 2)
            : 0m,
        StartDate       = c.StartDate,
        EndDate         = c.EndDate,
        Status          = c.Status,
        CreatedByUserId = c.CreatedByUserId,
        CreatedAt       = c.CreatedAt,
        UpdatedAt       = c.UpdatedAt,
    };
}
