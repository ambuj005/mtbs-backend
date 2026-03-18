using System.Text.Json.Serialization;

namespace MovieTicketBooking.Api.Models;

public class Show : BaseEntity
{
    [JsonPropertyName("movieId")]
    public string MovieId { get; set; } = string.Empty;

    [JsonPropertyName("theatreId")]
    public string TheatreId { get; set; } = string.Empty;

    [JsonPropertyName("screenNumber")]
    public int ScreenNumber { get; set; }

    [JsonPropertyName("screenType")]
    public string ScreenType { get; set; } = "2D";

    [JsonPropertyName("showTime")] 
    public DateTime ShowTime { get; set; }

    [JsonPropertyName("endTime")]
    public DateTime EndTime { get; set; }


    [JsonPropertyName("priceCategories")]
    public List<PriceCategory> PriceCategories { get; set; } = new();

    [JsonPropertyName("totalSeats")]
    public int TotalSeats { get; set; }

    [JsonPropertyName("availableSeats")]
    public int AvailableSeats { get; set; }

    [JsonPropertyName("status")] 
    public ShowStatus Status { get; set; }

    [JsonPropertyName("seats")]
    public List<ShowSeat> Seats { get; set; } = new();

    [JsonPropertyName("partitionKey")]
    public string PartitionKey { get; set; } = default!;
}


public class ShowSeat
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("row")]
    public string Row { get; set; } = string.Empty;

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("category")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SeatCategory Category { get; set; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SeatStatus Status { get; set; } = SeatStatus.Available;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("lockedUntil")]
    public DateTime? LockedUntil { get; set; }

    [JsonPropertyName("lockSessionId")]
    public string? LockSessionId { get; set; }

    [JsonPropertyName("bookingId")]
    public string? BookingId { get; set; }
}

public class PriceCategory
{
    [JsonPropertyName("category")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SeatCategory Category { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}

public enum ShowStatus { Scheduled, Running, Completed, Cancelled }
public enum SeatCategory { Platinum, Gold, Silver }
public enum SeatStatus { Available, Locked, Booked, Unavailable }