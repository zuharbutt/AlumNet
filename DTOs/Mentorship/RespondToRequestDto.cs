using System.ComponentModel.DataAnnotations;

namespace AlumniManagementSystem.DTOs.Mentorship;

public class RespondToRequestDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
