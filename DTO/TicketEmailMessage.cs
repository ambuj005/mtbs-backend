namespace MovieTicketBooking.Api.DTO;

public class TicketEmailMessage
{
    public string TicketId { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;

    public string BookingId { get; set; } = string.Empty;

    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public string MovieTitle { get; set; } = string.Empty;
    public string TheatreName { get; set; } = string.Empty;
    public DateTime ShowTime { get; set; }

    public List<string> Seats { get; set; } = new();

    public decimal TotalAmount { get; set; }
    public string QrCodeBase64 { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
