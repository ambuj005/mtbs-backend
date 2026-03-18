namespace MovieTicketBooking.Api.DTO;

public class MovieListItemDTO
{
    public string Id { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string PosterUrl { get; set; } = default!;
    public string BannerUrl { get; set; } = default!;
    public string Language { get; set; } = default!;
    public int Duration { get; set; }
    public string Rating { get; set; } = default!;
    public double ImdbRating { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Description { get; set; } = default!;
    public List<string> Genre { get; set; } = new();
    public int AvailableTheatresCount { get; set; }
    public string? TrailerUrl { get; set; } = default!;
}

