using System.Text.Json.Serialization;

namespace MovieTicketBooking.Api.Models;

using System.Text.Json.Serialization;

public class Ticket : BaseEntity
{
    [JsonPropertyName("ticketNumber")]
    public string TicketNumber { get; set; }
        = $"TKT{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(100000, 999999)}";

    [JsonPropertyName("bookingId")]
    public string BookingId { get; set; } = string.Empty;

    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("userEmail")]
    public string UserEmail { get; set; } = string.Empty;

    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("qrCode")]
    public string QrCode { get; set; } = string.Empty;

    [JsonPropertyName("qrData")]
    public string QrData { get; set; } = string.Empty;

    [JsonPropertyName("movie")]
    public TicketMovieInfo Movie { get; set; } = new();

    [JsonPropertyName("theatre")]
    public TicketTheatreInfo Theatre { get; set; } = new();

    [JsonPropertyName("show")]
    public TicketShowInfo Show { get; set; } = new();

    [JsonPropertyName("seats")]
    public List<BookedSeat> Seats { get; set; } = new();

    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TicketStatus Status { get; set; } = TicketStatus.Valid;

    [JsonPropertyName("issuedAt")]
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("validUntil")]
    public DateTime ValidUntil { get; set; }

    [JsonPropertyName("usedAt")]
    public DateTime? UsedAt { get; set; }

    [JsonPropertyName("verifiedBy")]
    public string? VerifiedBy { get; set; }

    [JsonPropertyName("partitionKey")]
    public string PartitionKey => BookingId;
}

public class TicketMovieInfo
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("posterUrl")] public string PosterUrl { get; set; } = string.Empty;
    [JsonPropertyName("duration")] public int Duration { get; set; }
    [JsonPropertyName("language")] public string Language { get; set; } = string.Empty;
    [JsonPropertyName("rating")] public string Rating { get; set; } = string.Empty;
}

public class TicketTheatreInfo
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("address")] public string Address { get; set; } = string.Empty;
    [JsonPropertyName("city")] public string City { get; set; } = string.Empty;
}

public class TicketShowInfo
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("showTime")] public DateTime ShowTime { get; set; }
    [JsonPropertyName("screenNumber")] public int ScreenNumber { get; set; }
    [JsonPropertyName("screenType")] public string ScreenType { get; set; } = string.Empty;
}

public enum TicketStatus { Valid, Used, Expired, Cancelled }
