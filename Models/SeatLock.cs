using System.Text.Json.Serialization;

namespace MovieTicketBooking.Api.Models;

public class SeatLock : BaseEntity
{
    [JsonPropertyName("showId")]
    public string ShowId { get; set; } = string.Empty;

    [JsonPropertyName("seatIds")]
    public List<string> SeatIds { get; set; } = new();

    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; set; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SeatLockStatus Status { get; set; } = SeatLockStatus.Active;

    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("ttl")]
    public int TtlSeconds { get; set; }

    [JsonPropertyName("razorpayOrderId")]
    public string RazorpayOrderId { get; set; } = string.Empty;

    [JsonPropertyName("partitionKey")]
    public string PartitionKey => ShowId;
}

public enum SeatLockStatus { Active, Expired, Released, Converted }
