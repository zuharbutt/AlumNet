using System.ComponentModel.DataAnnotations;

namespace AlumniManagementSystem.DTOs.Auth;

public class RegisterDto
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Student|Alumni|Admin)$", ErrorMessage = "Role must be Student, Alumni, or Admin.")]
    public string Role { get; set; } = string.Empty;

    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    // Only required when Role == "Admin"
    public string? InviteCode { get; set; }
}
