namespace AlumniManagementSystem.Entities;

public class Event
{
    public int      EventId         { get; set; }
    public string   Title           { get; set; } = string.Empty;
    public string   Description     { get; set; } = string.Empty;
    public string   Category        { get; set; } = string.Empty;
    public DateTime EventDate       { get; set; }
    public string   Location        { get; set; } = string.Empty;
    public int      Capacity        { get; set; }
    public decimal  TicketPrice     { get; set; } = 0;
    public string   Status          { get; set; } = "Scheduled";
    public int      CreatedByUserId { get; set; }
    public DateTime CreatedAt       { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt      { get; set; }

    // Navigation
    public User?                          CreatedBy      { get; set; }
    public ICollection<EventRegistration> Registrations  { get; set; } = [];
}
