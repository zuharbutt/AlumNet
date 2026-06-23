namespace AlumniManagementSystem.DTOs.Donation;

public class CampaignResponseDto
{
    public int      CampaignId      { get; set; }
    public string   Title           { get; set; } = string.Empty;
    public string   Description     { get; set; } = string.Empty;
    public decimal  TargetAmount    { get; set; }
    public decimal  RaisedAmount    { get; set; }
    public decimal  ProgressPercent { get; set; }
    public DateTime StartDate       { get; set; }
    public DateTime EndDate         { get; set; }
    public string   Status          { get; set; } = string.Empty;
    public int      CreatedByUserId { get; set; }
    public DateTime CreatedAt       { get; set; }
    public DateTime? UpdatedAt      { get; set; }
}
