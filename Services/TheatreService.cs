using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Services;

public class TheatreService : ITheatreService
{
    private readonly ITheatreRepository _theatreRepo;
    private readonly IShowRepository _showRepo;

    public TheatreService(ITheatreRepository theatreRepo, IShowRepository showRepo)
    {
        _theatreRepo = theatreRepo;
        _showRepo = showRepo;
    }

    public async Task<IEnumerable<Theatre>> GetTheatresAsync(string? cityId = null, string? areaId = null)
    {
        if (!string.IsNullOrEmpty(areaId)) return await _theatreRepo.GetByAreaIdAsync(areaId);
        if (!string.IsNullOrEmpty(cityId)) return await _theatreRepo.GetByCityIdAsync(cityId);
        return await _theatreRepo.GetAllAsync();
    }

    public Task<Theatre?> GetTheatreByIdAsync(string id, string cityId)
        => _theatreRepo.GetByIdAsync(id, cityId);

    public async Task<IEnumerable<ShowWithTheatreDTO>> GetTheatresWithShowsAsync(
        string movieId,
        string cityId,
        DateTime? date = null)
    {
        Console.WriteLine(">>> GetTheatresWithShowsAsync HIT <<<");
        Console.WriteLine($"movieId={movieId}, cityId={cityId}, date={date}");
         
        DateTime? normalizedDate = null;
        if (date.HasValue)
        {
            normalizedDate = new DateTime(
                date.Value.Year,
                date.Value.Month,
                date.Value.Day,
                0, 0, 0,
                DateTimeKind.Utc
            );
            Console.WriteLine($"normalized date = {normalizedDate} (UTC)");
            Console.WriteLine($"normalized date ticks = {normalizedDate.Value.Ticks}");
        }

        var theatres = await _theatreRepo.GetByCityIdAsync(cityId);
        Console.WriteLine($"theatres count = {theatres.Count()}");

        var result = new List<ShowWithTheatreDTO>();

        foreach (var t in theatres)
        {
            Console.WriteLine($"checking theatre {t.Id}");

            var shows = normalizedDate.HasValue
                ? await _showRepo.GetByMovieAndTheatreAsync(movieId, t.Id, normalizedDate.Value)
                : await _showRepo.GetByMovieAndTheatreAllAsync(movieId, t.Id);

            Console.WriteLine($"shows count for theatre {t.Id} = {shows.Count()}");

            result.AddRange(shows.Select(s => new ShowWithTheatreDTO
            {
                ShowId = s.Id,
                MovieId = s.MovieId,
                ShowTime = s.ShowTime,
                EndTime = s.EndTime,
                ScreenNumber = s.ScreenNumber,
                ScreenType = s.ScreenType,
                AvailableSeats = s.AvailableSeats,
                TotalSeats = s.TotalSeats,
                PriceCategories = s.PriceCategories,
                Status = s.Status,
                Theatre = new TheatreDTO
                {
                    Id = t.Id,
                    Name = t.Name,
                    Address = t.Address,
                    Facilities = t.Facilities,
                    Rating = t.Rating
                }
            }));
        }

        Console.WriteLine($"TOTAL RESULT COUNT = {result.Count}");
        return result;
    }
}
