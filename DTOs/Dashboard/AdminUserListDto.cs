namespace AlumniManagementSystem.DTOs.Dashboard;

public class AdminUserItemDto
{
    public int     UserId         { get; set; }
    public string  FullName       { get; set; } = string.Empty;
    public string  Email          { get; set; } = string.Empty;
    public string  Department     { get; set; } = string.Empty;
    public string  Status         { get; set; } = string.Empty;
    public DateTime JoinedAt      { get; set; }
}

public class AdminUserListDto
{
    public List<AdminUserItemDto> Users    { get; set; } = [];
    public int                   Total    { get; set; }
    public int                   Page     { get; set; }
    public int                   PageSize { get; set; }
    public int                   TotalPages { get; set; }
}
