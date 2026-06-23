namespace AlumniManagementSystem.Entities;

public class MentorshipRequest
{
    public int      RequestId       { get; set; }
    public int      StudentUserId   { get; set; }
    public int      AlumniUserId    { get; set; }
    public string   Message         { get; set; } = string.Empty;
    public string   Status          { get; set; } = "Pending";
    public DateTime RequestedAt     { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt    { get; set; }
    public string?  ResponseMessage { get; set; }

    // Navigation
    public User? Student { get; set; }
    public User? Alumni  { get; set; }
}
