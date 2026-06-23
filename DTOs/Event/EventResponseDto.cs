namespace AlumniManagementSystem.DTOs.Event;

public class EventResponseDto
{
    public int      EventId          { get; set; }
    public string   Title            { get; set; } = string.Empty;
    public string   Description      { get; set; } = string.Empty;
    public string   Category         { get; set; } = string.Empty;
    public DateTime EventDate        { get; set; }
    public string   Location         { get; set; } = string.Empty;
    public int      Capacity         { get; set; }
    public decimal  TicketPrice      { get; set; }
    public string   Status           { get; set; } = string.Empty;
    public int      RegisteredCount  { get; set; }
    public int      SeatsRemaining   { get; set; }
    public bool     IsFull           { get; set; }
    public bool?    IsRegistered     { get; set; }   // null = not checked (anonymous)
    public DateTime CreatedAt        { get; set; }
    public DateTime? UpdatedAt       { get; set; }
    public string   CreatedByName    { get; set; } = string.Empty;
}
