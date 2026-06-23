namespace AlumniManagementSystem.DTOs.Dashboard;

public class EventReportDto
{
    public int                       TotalEvents        { get; set; }
    public int                       TotalRegistrations { get; set; }
    public List<EventReportItemDto>  Events             { get; set; } = [];
}

public class EventReportItemDto
{
    public int     EventId          { get; set; }
    public string  Title            { get; set; } = string.Empty;
    public DateTime EventDate       { get; set; }
    public int     Capacity         { get; set; }
    public int     TotalRegistered  { get; set; }
    public int     TotalAttended    { get; set; }
    public int     TotalCancelled   { get; set; }
    public decimal AttendanceRate   { get; set; }
}
