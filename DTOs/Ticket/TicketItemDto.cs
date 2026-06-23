namespace AlumniManagementSystem.DTOs.Ticket;

public class TicketItemDto
{
    public int      TicketId        { get; set; }
    public int      RegistrationId  { get; set; }
    public string   TicketCode      { get; set; } = string.Empty;
    public decimal  Price           { get; set; }
    public string   PaymentStatus   { get; set; } = string.Empty;
    public DateTime IssuedAt        { get; set; }
    public DateTime? RefundedAt     { get; set; }
    public DateTime? CancelledAt    { get; set; }

    // Event info
    public int      EventId         { get; set; }
    public string   EventTitle      { get; set; } = string.Empty;
    public DateTime EventDate       { get; set; }
    public string   EventLocation   { get; set; } = string.Empty;
    public string   EventCategory   { get; set; } = string.Empty;
    public string   EventStatus     { get; set; } = string.Empty;

    public bool CanCancel => CancelledAt == null
                             && EventDate > DateTime.UtcNow.AddHours(24)
                             && EventStatus == "Scheduled";
}
