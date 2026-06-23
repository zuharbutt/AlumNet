namespace AlumniManagementSystem.Entities;

public class Donation
{
    public int      DonationId  { get; set; }
    public int      CampaignId  { get; set; }
    public int      DonorUserId { get; set; }
    public decimal  Amount      { get; set; }
    public bool     IsAnonymous { get; set; } = false;
    public string   Status      { get; set; } = "Completed";
    public DateTime DonatedAt   { get; set; } = DateTime.UtcNow;

    // Navigation
    public DonationCampaign? Campaign { get; set; }
    public User?             Donor    { get; set; }
}
