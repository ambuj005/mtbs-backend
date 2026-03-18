using System.Text.Json.Serialization;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.DTO;

public class CreateBookingRequest
{
    public string ShowId { get; set; } = string.Empty;
    public string TheatreId { get; set; } = string.Empty;
    public string MovieId { get; set; } = string.Empty;
    public List<string> SeatIds { get; set; } = new();
    public string SeatLockId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserPhone { get; set; }
}

public class BookingResponse
{
    public string BookingId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))] public BookingStatus Status { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string TheatreName { get; set; } = string.Empty;
    public DateTime ShowTime { get; set; }
    public int ScreenNumber { get; set; }
    public List<BookedSeat> Seats { get; set; } = new();
    public decimal BaseAmount { get; set; }
    public decimal ConvenienceFee { get; set; }
    public decimal Taxes { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CancelBookingRequest { public string? Reason { get; set; } }
