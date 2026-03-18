using System.Text.Json.Serialization;

namespace MovieTicketBooking.Api.Models;

public class Theatre : BaseEntity
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("areaId")]
    public string AreaId { get; set; } = string.Empty;

    [JsonPropertyName("cityId")]
    public string CityId { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("facilities")]
    public List<string> Facilities { get; set; } = new();

    [JsonPropertyName("totalScreens")]
    public int TotalScreens { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("rating")]
    public decimal? Rating { get; set; }

    [JsonPropertyName("partitionKey")]
    public string PartitionKey => CityId;
}
