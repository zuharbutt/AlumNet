namespace AlumniManagementSystem.DTOs.Donation;

public class CreateCampaignDto
{
    public string  Title         { get; set; } = string.Empty;
    public string  Description   { get; set; } = string.Empty;
    public decimal TargetAmount  { get; set; }
    public DateTime EndDate      { get; set; }
    public string  Status        { get; set; } = "Active";
}
