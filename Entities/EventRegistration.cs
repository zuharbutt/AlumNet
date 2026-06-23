namespace AlumniManagementSystem.Entities;

public class EventRegistration
{
    public int      RegistrationId { get; set; }
    public int      EventId        { get; set; }
    public int      UserId         { get; set; }
    public DateTime RegisteredAt   { get; set; } = DateTime.UtcNow;
    public bool     Attended       { get; set; } = false;
    public DateTime? CancelledAt   { get; set; }

    // Navigation
    public Event?  Event  { get; set; }
    public User?   User   { get; set; }
    public Ticket? Ticket { get; set; }
}
