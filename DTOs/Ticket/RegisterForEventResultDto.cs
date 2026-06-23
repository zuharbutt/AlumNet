namespace AlumniManagementSystem.DTOs.Ticket;

public class RegisterForEventResultDto
{
    public int    TicketId   { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public decimal Price     { get; set; }
}
