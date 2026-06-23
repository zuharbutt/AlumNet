namespace AlumniManagementSystem.DTOs.Student;

public class StudentProfileDto
{
    public int      UserId                 { get; set; }
    public string   FullName               { get; set; } = string.Empty;
    public string   Email                  { get; set; } = string.Empty;
    public string?  Phone                  { get; set; }
    public int      DepartmentId           { get; set; }
    public string   DepartmentName         { get; set; } = string.Empty;
    public int      EnrollmentYear         { get; set; }
    public int      ExpectedGraduationYear { get; set; }
    public string   DegreeProgram          { get; set; } = string.Empty;
    public int?     CurrentSemester        { get; set; }
    public decimal? CGPA                   { get; set; }
    public string?  ShortBio               { get; set; }
}
