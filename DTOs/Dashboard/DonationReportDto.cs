namespace AlumniManagementSystem.DTOs.Dashboard;

public class DonationReportDto
{
    public int                          TotalCampaigns  { get; set; }
    public decimal                      TotalRaised     { get; set; }
    public int                          TotalDonors     { get; set; }
    public List<DonationReportItemDto>  Campaigns       { get; set; } = [];
}

public class DonationReportItemDto
{
    public int     CampaignId       { get; set; }
    public string  Title            { get; set; } = string.Empty;
    public decimal TargetAmount     { get; set; }
    public decimal RaisedAmount     { get; set; }
    public int     DonorCount       { get; set; }
    public decimal AverageDonation  { get; set; }
    public decimal LargestDonation  { get; set; }
}
