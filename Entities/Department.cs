namespace AlumniManagementSystem.Entities;

public class Department
{
    public int     DepartmentId { get; set; }
    public string  Name         { get; set; } = string.Empty;
    public string? Description  { get; set; }
    public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<AlumniProfile>  AlumniProfiles  { get; set; } = [];
    public ICollection<StudentProfile> StudentProfiles { get; set; } = [];
}
