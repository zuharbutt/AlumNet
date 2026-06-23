using AlumniManagementSystem.Data;
using AlumniManagementSystem.DTOs.Donation;
using AlumniManagementSystem.Entities;
using AlumniManagementSystem.Enums;
using Microsoft.EntityFrameworkCore;

namespace AlumniManagementSystem.Services;

public class DonationService(AppDbContext db)
{
    // ── POST /api/donations (Alumni only) ─────────────────────────────────────
    // NOTE: DB trigger tr_Donations_AfterInsert auto-updates RaisedAmount. Never do it here.
    public async Task<int> MakeDonationAsync(int donorUserId, MakeDonationDto dto)
    {
        if (dto.Amount <= 0)
            throw new ArgumentException("Amount must be greater than 0.");

        if (dto.Amount > 1_000_000)
            throw new ArgumentException("Amount exceeds maximum of 1,000,000.");

        var campaign = await db.DonationCampaigns
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CampaignId == dto.CampaignId)
            ?? throw new KeyNotFoundException("Campaign not found.");

        if (campaign.Status == CampaignStatus.Cancelled)
            throw new InvalidOperationException("Campaign not accepting donations.");

        if (campaign.Status == CampaignStatus.Closed || campaign.EndDate <= DateTime.UtcNow)
            throw new InvalidOperationException("Campaign has ended.");

        if (campaign.Status != CampaignStatus.Active)
            throw new InvalidOperationException("Campaign is not active.");

        var remaining = campaign.TargetAmount - campaign.RaisedAmount;
        if (remaining <= 0)
            throw new InvalidOperationException("Campaign has already reached its target.");

        if (dto.Amount > remaining)
            throw new InvalidOperationException(
                $"Donation exceeds remaining target. Maximum you can donate: Rs {remaining:F0}.");

        var donation = new Donation
        {
            CampaignId  = dto.CampaignId,
            DonorUserId = donorUserId,
            Amount      = dto.Amount,
            IsAnonymous = dto.IsAnonymous,
            Status      = "Completed",
            DonatedAt   = DateTime.UtcNow,
        };

        db.Donations.Add(donation);
        await db.SaveChangesAsync();
        // Trigger tr_Donations_AfterInsert fires in the DB and updates RaisedAmount.

        // Auto-close campaign if target is fully reached
        var updatedCampaign = await db.DonationCampaigns
            .FirstOrDefaultAsync(c => c.CampaignId == dto.CampaignId);
        if (updatedCampaign != null
            && updatedCampaign.Status == CampaignStatus.Active
            && updatedCampaign.RaisedAmount >= updatedCampaign.TargetAmount)
        {
            updatedCampaign.Status    = CampaignStatus.Closed;
            updatedCampaign.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        return donation.DonationId;
    }

    // ── GET /api/donations/mine (Alumni only) ─────────────────────────────────
    public async Task<DonationListResultDto> GetMyDonationsAsync(int userId, int page, int pageSize)
    {
        var query = db.Donations
            .AsNoTracking()
            .Include(d => d.Campaign)
            .Include(d => d.Donor)
            .Where(d => d.DonorUserId == userId);

        return await BuildListResultAsync(query, page, pageSize);
    }

    // ── GET /api/admin/donations (Admin only) ─────────────────────────────────
    public async Task<DonationListResultDto> GetAllDonationsAsync(int page, int pageSize)
    {
        var query = db.Donations
            .AsNoTracking()
            .Include(d => d.Campaign)
            .Include(d => d.Donor);

        return await BuildListResultAsync(query, page, pageSize);
    }

    // ─────────────────────────────────────────────────────────────────────────
    private static async Task<DonationListResultDto> BuildListResultAsync(
        IQueryable<Donation> query, int page, int pageSize)
    {
        var total       = await query.CountAsync();
        var totalAmount = await query.SumAsync(d => (decimal?)d.Amount) ?? 0m;
        var offset      = (page - 1) * pageSize;

        var items = await query
            .OrderByDescending(d => d.DonatedAt)
            .Skip(offset)
            .Take(pageSize)
            .Select(d => new DonationItemDto
            {
                DonationId    = d.DonationId,
                CampaignId    = d.CampaignId,
                CampaignTitle = d.Campaign != null ? d.Campaign.Title : string.Empty,
                DonorUserId   = d.DonorUserId,
                DonorName     = d.IsAnonymous
                    ? "Anonymous"
                    : (d.Donor != null ? d.Donor.FullName : "Unknown"),
                Amount        = d.Amount,
                IsAnonymous   = d.IsAnonymous,
                Status        = d.Status,
                DonatedAt     = d.DonatedAt,
            })
            .ToListAsync();

        return new DonationListResultDto
        {
            Items       = items,
            TotalCount  = total,
            Page        = page,
            PageSize    = pageSize,
            TotalAmount = totalAmount,
        };
    }
}
