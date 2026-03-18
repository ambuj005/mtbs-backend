using MovieTicketBooking.Api.Models;
using System.Text.Json.Serialization;

namespace MovieTicketBooking.Api.DTO;

public record SeatDTO
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("showId")]
    public string ShowId { get; init; } = string.Empty;

    [JsonPropertyName("row")]
    public string Row { get; init; } = string.Empty;

    [JsonPropertyName("number")]
    public int Number { get; init; }

    [JsonPropertyName("category")]
    public string Category { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SeatStatus Status { get; init; }

    [JsonPropertyName("price")]
    public decimal Price { get; init; }

    [JsonPropertyName("isLocked")]
    public bool IsLocked { get; init; }

    [JsonPropertyName("lockedUntil")]
    public DateTime? LockedUntil { get; init; }

    [JsonPropertyName("lockSessionId")]
    public string? LockSessionId { get; init; }  
}

public record SeatRowDTO
{
    [JsonPropertyName("rowLabel")]
    public string RowLabel { get; init; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; init; } = string.Empty;

    [JsonPropertyName("seats")]
    public List<SeatDTO> Seats { get; init; } = new();
}

public record SeatLayoutResponse
{
    [JsonPropertyName("showId")]
    public string ShowId { get; init; } = string.Empty;

    [JsonPropertyName("screenInfo")]
    public ScreenInfo ScreenInfo { get; init; } = new();

    [JsonPropertyName("rows")]
    public List<SeatRowDTO> Rows { get; init; } = new();

    [JsonPropertyName("priceCategories")]
    public List<PriceCategory> PriceCategories { get; init; } = new();
}

public record ScreenInfo
{
    [JsonPropertyName("screenNumber")]
    public int ScreenNumber { get; init; }

    [JsonPropertyName("screenType")]
    public string ScreenType { get; init; } = string.Empty;

    [JsonPropertyName("totalSeats")]
    public int TotalSeats { get; init; }

    [JsonPropertyName("availableSeats")]
    public int AvailableSeats { get; init; }
}

public record LockSeatsRequest
{
    [JsonPropertyName("showId")]
    public string ShowId { get; init; } = string.Empty;

    [JsonPropertyName("seatIds")]
    public List<string> SeatIds { get; init; } = new();

    [JsonPropertyName("sessionId")]
    public string SessionId { get; init; } = string.Empty;
}

public record LockSeatsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("lockId")]
    public string LockId { get; init; } = string.Empty;

    [JsonPropertyName("lockedSeats")]
    public List<string> LockedSeats { get; init; } = new();

    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("unavailableSeats")]
    public List<string>? UnavailableSeats { get; init; }
}

public record UnlockSeatsRequest
{
    [JsonPropertyName("lockId")]
    public string LockId { get; init; } = string.Empty;

    [JsonPropertyName("sessionId")]
    public string SessionId { get; init; } = string.Empty;
}

public record ExtendLockRequest
{
    [JsonPropertyName("lockId")]
    public string LockId { get; init; } = string.Empty;

    [JsonPropertyName("sessionId")]
    public string SessionId { get; init; } = string.Empty;

    [JsonPropertyName("additionalMinutes")]
    public int AdditionalMinutes { get; init; } = 5;
}