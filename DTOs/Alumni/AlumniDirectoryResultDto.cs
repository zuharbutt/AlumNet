namespace AlumniManagementSystem.DTOs.Alumni;

public class AlumniDirectoryResultDto
{
    public List<AlumniDirectoryItemDto> Items      { get; set; } = [];
    public int                          TotalCount { get; set; }
    public int                          PageNumber { get; set; }
    public int                          PageSize   { get; set; }
}
