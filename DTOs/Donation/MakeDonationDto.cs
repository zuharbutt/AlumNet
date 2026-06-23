namespace AlumniManagementSystem.DTOs.Donation;

public class MakeDonationDto
{
    public int     CampaignId  { get; set; }
    public decimal Amount      { get; set; }
    public bool    IsAnonymous { get; set; } = false;
}
