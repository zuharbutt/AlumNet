using System.ComponentModel.DataAnnotations;

namespace AlumniManagementSystem.DTOs.Alumni;

public class UpdateAlumniProfileDto
{
    [Required]
    public int DepartmentId { get; set; }

    [Required]
    [Range(1950, 2100)]
    public int GraduationYear { get; set; }

    [Required]
    [MaxLength(100)]
    public string DegreeProgram { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? CurrentCompany { get; set; }

    [MaxLength(100)]
    public string? JobTitle { get; set; }

    [MaxLength(100)]
    public string? Industry { get; set; }

    [MaxLength(150)]
    public string? WorkLocation { get; set; }

    [MaxLength(300)]
    public string? LinkedInUrl { get; set; }

    [MaxLength(1000)]
    public string? ShortBio { get; set; }
}
