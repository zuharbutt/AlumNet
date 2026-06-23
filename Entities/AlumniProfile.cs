namespace AlumniManagementSystem.Entities;

public class AlumniProfile
{
    public int      AlumniProfileId { get; set; }
    public int      UserId          { get; set; }
    public int      DepartmentId    { get; set; }
    public int      GraduationYear  { get; set; }
    public string   DegreeProgram   { get; set; } = string.Empty;
    public string?  CurrentCompany  { get; set; }
    public string?  JobTitle        { get; set; }
    public string?  Industry        { get; set; }
    public string?  WorkLocation    { get; set; }
    public string?  LinkedInUrl     { get; set; }
    public string?  ShortBio        { get; set; }
    public DateTime CreatedAt       { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt      { get; set; }

    // Navigation
    public User?       User       { get; set; }
    public Department? Department { get; set; }
}
