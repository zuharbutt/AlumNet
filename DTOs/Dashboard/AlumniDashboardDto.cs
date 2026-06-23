namespace AlumniManagementSystem.DTOs.Dashboard;

public class AlumniDashboardDto
{
    public int     IncomingRequestsCount { get; set; }
    public int     AcceptedMenteeCount   { get; set; }
    public decimal TotalDonated          { get; set; }
    public int     UpcomingEventsCount   { get; set; }
}
