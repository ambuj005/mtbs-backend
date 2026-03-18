using System.Text.Json.Serialization;

namespace MovieTicketBooking.Api.Models;

public class City : BaseEntity
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; set; } = "India";

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("isPopular")]
    public bool IsPopular { get; set; }

    [JsonPropertyName("partitionKey")]
    public string PartitionKey => Country;
}
