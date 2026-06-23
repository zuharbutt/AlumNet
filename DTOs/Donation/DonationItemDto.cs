namespace AlumniManagementSystem.DTOs.Donation;

public class DonationItemDto
{
    public int      DonationId    { get; set; }
    public int      CampaignId    { get; set; }
    public string   CampaignTitle { get; set; } = string.Empty;
    public string   DonorName     { get; set; } = string.Empty;
    public int      DonorUserId   { get; set; }
    public decimal  Amount        { get; set; }
    public bool     IsAnonymous   { get; set; }
    public string   Status        { get; set; } = string.Empty;
    public DateTime DonatedAt     { get; set; }
}
