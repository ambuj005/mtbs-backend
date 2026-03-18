using System.Text.Json.Serialization;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.DTO;

public class TicketResponse
{
    public string TicketId { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;
    public string BookingId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
    public TicketMovieInfo Movie { get; set; } = new();
    public TicketTheatreInfo Theatre { get; set; } = new();
    public TicketShowInfo Show { get; set; } = new();
    public List<BookedSeat> Seats { get; set; } = new();
    public decimal TotalAmount { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))] public TicketStatus Status { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime ValidUntil { get; set; }
}

public class VerifyQRRequest
{
    public string QrData { get; set; } = string.Empty;
    public string TicketId { get; set; } = string.Empty;
    public string TheatreId { get; set; } = string.Empty;
    public string VerifiedBy { get; set; } = string.Empty;
    public bool MarkAsUsed { get; set; } = true;
}

public class VerifyQRResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public TicketResponse? Ticket { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public List<string>? ValidationErrors { get; set; }
}
