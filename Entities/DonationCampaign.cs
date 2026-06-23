namespace AlumniManagementSystem.Entities;

public class DonationCampaign
{
    public int      CampaignId      { get; set; }
    public string   Title           { get; set; } = string.Empty;
    public string   Description     { get; set; } = string.Empty;
    public decimal  TargetAmount    { get; set; }
    public decimal  RaisedAmount    { get; set; } = 0;
    public DateTime StartDate       { get; set; } = DateTime.UtcNow;
    public DateTime EndDate         { get; set; }
    public string   Status          { get; set; } = "Active";
    public int      CreatedByUserId { get; set; }
    public DateTime CreatedAt       { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt      { get; set; }

    // Navigation
    public User?                  CreatedBy { get; set; }
    public ICollection<Donation>  Donations { get; set; } = [];
}
