namespace AlumniManagementSystem.Entities;

public class User
{
    public int       UserId         { get; set; }
    public string    FullName       { get; set; } = string.Empty;
    public string    Email          { get; set; } = string.Empty;
    public string    PasswordHash   { get; set; } = string.Empty;
    public string?   Phone          { get; set; }
    public DateOnly? DateOfBirth    { get; set; }
    public string    Role           { get; set; } = string.Empty;
    public bool      IsActive       { get; set; } = true;
    public int       FailedAttempts { get; set; } = 0;
    public DateTime? LockedUntil    { get; set; }
    public DateTime  CreatedAt      { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt      { get; set; }

    // Navigation
    public AlumniProfile?    AlumniProfile    { get; set; }
    public StudentProfile?   StudentProfile   { get; set; }
}
