using System.Text.Json.Serialization;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.DTO;

public class ShowWithTheatreDTO
{
    public string ShowId { get; set; } = string.Empty;
    public string MovieId { get; set; } = string.Empty;
    public DateTime ShowTime { get; set; }
    public DateTime EndTime { get; set; }
    public int ScreenNumber { get; set; }
    public string ScreenType { get; set; } = string.Empty;
    public int AvailableSeats { get; set; }
    public int TotalSeats { get; set; }
    public List<PriceCategory> PriceCategories { get; set; } = new();
    [JsonConverter(typeof(JsonStringEnumConverter))] public ShowStatus Status { get; set; }
    public TheatreDTO Theatre { get; set; } = new();
}

public class TheatreDTO
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public List<string> Facilities { get; set; } = new();
    public decimal? Rating { get; set; }
}
