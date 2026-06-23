namespace AlumniManagementSystem.DTOs.Dashboard;

public class AdminMentorshipItemDto
{
    public int      RequestId       { get; set; }
    public string   StudentName     { get; set; } = string.Empty;
    public string   StudentEmail    { get; set; } = string.Empty;
    public string   AlumniName      { get; set; } = string.Empty;
    public string   AlumniEmail     { get; set; } = string.Empty;
    public string   Status          { get; set; } = string.Empty;
    public DateTime RequestedAt     { get; set; }
    public string   Message         { get; set; } = string.Empty;
}

public class AdminMentorshipListDto
{
    public List<AdminMentorshipItemDto> Requests   { get; set; } = [];
    public int                          Total      { get; set; }
    public int                          Page       { get; set; }
    public int                          PageSize   { get; set; }
    public int                          TotalPages { get; set; }
}
