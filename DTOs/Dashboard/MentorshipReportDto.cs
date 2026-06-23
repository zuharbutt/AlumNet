namespace AlumniManagementSystem.DTOs.Dashboard;

public class MentorshipReportDto
{
    public int TotalRequests  { get; set; }
    public int Pending        { get; set; }
    public int Accepted       { get; set; }
    public int Rejected       { get; set; }
    public int Cancelled      { get; set; }
}
