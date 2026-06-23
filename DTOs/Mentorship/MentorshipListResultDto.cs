namespace AlumniManagementSystem.DTOs.Mentorship;

public class MentorshipListResultDto
{
    public List<MentorshipRequestItemDto> Items      { get; set; } = [];
    public int                            TotalCount { get; set; }
    public int                            PageNumber { get; set; }
    public int                            PageSize   { get; set; }
}
