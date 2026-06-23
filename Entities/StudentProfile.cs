namespace AlumniManagementSystem.Entities;

public class StudentProfile
{
    public int      StudentProfileId       { get; set; }
    public int      UserId                 { get; set; }
    public int      DepartmentId           { get; set; }
    public int      EnrollmentYear         { get; set; }
    public int      ExpectedGraduationYear { get; set; }
    public string   DegreeProgram          { get; set; } = string.Empty;
    public int?     CurrentSemester        { get; set; }
    public decimal? CGPA                   { get; set; }
    public string?  ShortBio               { get; set; }
    public DateTime CreatedAt              { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt             { get; set; }

    // Navigation
    public User?       User       { get; set; }
    public Department? Department { get; set; }
}
