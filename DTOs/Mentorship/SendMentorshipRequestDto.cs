using System.ComponentModel.DataAnnotations;

namespace AlumniManagementSystem.DTOs.Mentorship;

public class SendMentorshipRequestDto
{
    [Required]
    public int AlumniId { get; set; }

    [Required]
    [MinLength(20, ErrorMessage = "Message must be at least 20 characters.")]
    [MaxLength(500, ErrorMessage = "Message cannot exceed 500 characters.")]
    public string Message { get; set; } = string.Empty;
}
