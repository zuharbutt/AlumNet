namespace AlumniManagementSystem.Entities;

public class AdminInviteCode
{
    public int      CodeId          { get; set; }
    public string   Code            { get; set; } = string.Empty;
    public bool     IsUsed          { get; set; } = false;
    public DateTime ExpiryDate      { get; set; }
    public int?     CreatedByUserId { get; set; }
    public int?     UsedByUserId    { get; set; }
    public DateTime CreatedAt       { get; set; } = DateTime.UtcNow;
    public DateTime? UsedAt         { get; set; }

    // Navigation
    public User? CreatedBy { get; set; }
    public User? UsedBy    { get; set; }
}
