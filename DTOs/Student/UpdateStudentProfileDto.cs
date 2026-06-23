using System.ComponentModel.DataAnnotations;

namespace AlumniManagementSystem.DTOs.Student;

public class UpdateStudentProfileDto
{
    [Required]
    public int DepartmentId { get; set; }

    [Required]
    [Range(2000, 2100)]
    public int EnrollmentYear { get; set; }

    [Required]
    public int ExpectedGraduationYear { get; set; }

    [Required]
    [MaxLength(100)]
    public string DegreeProgram { get; set; } = string.Empty;

    [Range(1, 12)]
    public int? CurrentSemester { get; set; }

    [Range(0.0, 4.0)]
    public decimal? CGPA { get; set; }

    [MaxLength(1000)]
    public string? ShortBio { get; set; }
}
