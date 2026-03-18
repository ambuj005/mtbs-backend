using System.Text.Json.Serialization;

namespace MovieTicketBooking.Api.Models;

public class Booking : BaseEntity
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

    [JsonPropertyName("userEmail")]
    public string UserEmail { get; set; } = string.Empty;

    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("userPhone")]
    public string? UserPhone { get; set; }

    [JsonPropertyName("movieId")]
    public string MovieId { get; set; } = string.Empty;

    [JsonPropertyName("movieTitle")]
    public string MovieTitle { get; set; } = string.Empty;

    [JsonPropertyName("theatreId")]
    public string TheatreId { get; set; } = string.Empty;

    [JsonPropertyName("theatreName")]
    public string TheatreName { get; set; } = string.Empty;

    [JsonPropertyName("showId")]
    public string ShowId { get; set; } = string.Empty;

    [JsonPropertyName("showTime")]
    public DateTime ShowTime { get; set; }

    [JsonPropertyName("screenNumber")]
    public int ScreenNumber { get; set; }

    [JsonPropertyName("seats")]
    public List<BookedSeat> Seats { get; set; } = new();

    [JsonPropertyName("baseAmount")]
    public decimal BaseAmount { get; set; }

    [JsonPropertyName("convenienceFee")]
    public decimal ConvenienceFee { get; set; }

    [JsonPropertyName("taxes")]
    public decimal Taxes { get; set; }

    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    [JsonPropertyName("paymentId")]
    public string? PaymentId { get; set; }

    [JsonPropertyName("paymentMethod")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PaymentMethod? PaymentMethod { get; set; }

    [JsonPropertyName("seatLockId")]
    public string? SeatLockId { get; set; }

    [JsonPropertyName("confirmedAt")]
    public DateTime? ConfirmedAt { get; set; }

    [JsonPropertyName("cancelledAt")]
    public DateTime? CancelledAt { get; set; }

    [JsonPropertyName("cancellationReason")]
    public string? CancellationReason { get; set; }

    [JsonPropertyName("partitionKey")]
    public string PartitionKey => ShowId;
}

public class BookedSeat
{
    [JsonPropertyName("seatId")]
    public string SeatId { get; set; } = string.Empty;

    [JsonPropertyName("row")]
    public string Row { get; set; } = string.Empty;

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("category")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SeatCategory Category { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}

public enum BookingStatus { Pending, Confirmed, Cancelled, Expired, PaymentFailed, Refunded }
public enum PaymentMethod { Card, Upi, NetBanking, Wallet }
