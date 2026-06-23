namespace AlumniManagementSystem.DTOs.Donation;

public class DonationListResultDto
{
    public List<DonationItemDto> Items      { get; set; } = [];
    public int                   TotalCount { get; set; }
    public int                   Page       { get; set; }
    public int                   PageSize   { get; set; }
    public decimal               TotalAmount { get; set; }
}
