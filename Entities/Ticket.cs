namespace AlumniManagementSystem.Entities;

public class Ticket
{
    public int      TicketId       { get; set; }
    public int      RegistrationId { get; set; }
    public string   TicketCode     { get; set; } = string.Empty;
    public decimal  Price          { get; set; }
    public string   PaymentStatus  { get; set; } = "Paid";
    public DateTime IssuedAt       { get; set; } = DateTime.UtcNow;
    public DateTime? RefundedAt    { get; set; }

    // Navigation
    public EventRegistration? Registration { get; set; }
}
