using System.Text.Json.Serialization;

namespace MovieTicketBooking.Api.Models;

public class Movie : BaseEntity
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("genre")]
    public List<string> Genre { get; set; } = new();

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("releaseDate")]
    public DateTime ReleaseDate { get; set; }

    [JsonPropertyName("rating")]
    public string Rating { get; set; } = "U";

    [JsonPropertyName("posterUrl")]
    public string PosterUrl { get; set; } = string.Empty;

    [JsonPropertyName("bannerUrl")]
    public string? BannerUrl { get; set; }

    [JsonPropertyName("trailerUrl")]
    public string? TrailerUrl { get; set; }

    [JsonPropertyName("cast")]
    public List<CastMember> Cast { get; set; } = new();

    [JsonPropertyName("director")]
    public string Director { get; set; } = string.Empty;

    [JsonPropertyName("imdbRating")]
    public decimal? ImdbRating { get; set; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MovieStatus Status { get; set; } = MovieStatus.NowShowing;

    [JsonPropertyName("partitionKey")]
    public string PartitionKey => Language;
}

public class CastMember
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }
}

public enum MovieStatus { ComingSoon, NowShowing, Ended }
