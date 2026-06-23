namespace AlumniManagementSystem.DTOs.Alumni;

public class AlumniDetailDto
{
    public int     UserId         { get; set; }
    public string  FullName       { get; set; } = string.Empty;
    public string? Email          { get; set; }   // null unless contact visible
    public string? Phone          { get; set; }   // null unless contact visible
    public bool    ContactVisible { get; set; }
    public int     GraduationYear { get; set; }
    public string  DepartmentName { get; set; } = string.Empty;
    public string  DegreeProgram  { get; set; } = string.Empty;
    public string? CurrentCompany { get; set; }
    public string? JobTitle       { get; set; }
    public string? Industry       { get; set; }
    public string? WorkLocation   { get; set; }
    public string? LinkedInUrl    { get; set; }
    public string? ShortBio       { get; set; }
}
