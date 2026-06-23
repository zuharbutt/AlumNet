namespace AlumniManagementSystem.DTOs.Alumni;

public class AlumniDirectoryItemDto
{
    public int     UserId         { get; set; }
    public string  FullName       { get; set; } = string.Empty;
    public int     GraduationYear { get; set; }
    public string  DepartmentName { get; set; } = string.Empty;
    public string? CurrentCompany { get; set; }
    public string? JobTitle       { get; set; }
    public string? Industry       { get; set; }
    public string? WorkLocation   { get; set; }
}
