namespace AlumniManagementSystem.DTOs.Mentorship;

public class MentorshipRequestItemDto
{
    public int     RequestId     { get; set; }
    public int     StudentUserId { get; set; }
    public string  StudentName   { get; set; } = string.Empty;
    public string? StudentEmail  { get; set; }
    public int     AlumniUserId  { get; set; }
    public string  AlumniName    { get; set; } = string.Empty;
    public string? AlumniEmail   { get; set; }
    public string? AlumniPhone   { get; set; }
    public string  Message       { get; set; } = string.Empty;
    public string  Status        { get; set; } = string.Empty;
    public DateTime RequestedAt  { get; set; }
    public DateTime? RespondedAt { get; set; }
}
