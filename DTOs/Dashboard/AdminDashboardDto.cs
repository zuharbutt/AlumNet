namespace AlumniManagementSystem.DTOs.Dashboard;

public class AdminDashboardDto
{
    public int     TotalAlumni             { get; set; }
    public int     TotalStudents           { get; set; }
    public int     UpcomingEvents          { get; set; }
    public int     ActiveCampaigns         { get; set; }
    public decimal TotalRaised             { get; set; }
    public int     TotalDonations          { get; set; }
    public int     PendingMentorships      { get; set; }
}
