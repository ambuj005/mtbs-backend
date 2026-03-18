using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Services;

public class ShowService : IShowService
{
    private readonly IShowRepository _showRepo;

    public ShowService(IShowRepository showRepo) { _showRepo = showRepo; }
    public async Task<IEnumerable<Show>> GetShowsAsync(
        string? movieId = null,
        string? theatreId = null,
        DateTime? date = null)
    {
        DateTime? normalizedDate = null;

        if (!string.IsNullOrEmpty(movieId) && !string.IsNullOrEmpty(theatreId))
            return await _showRepo.GetByMovieAndTheatreAsync(
                movieId,
                theatreId,
                normalizedDate
            );

        if (!string.IsNullOrEmpty(movieId))
            return await _showRepo.GetByMovieIdAsync(movieId, normalizedDate);

        if (!string.IsNullOrEmpty(theatreId))
            return await _showRepo.GetByTheatreIdAsync(theatreId, normalizedDate);

        return Enumerable.Empty<Show>();
    }

    public Task<Show?> GetShowByIdAsync(string id, string theatreId) => _showRepo.GetByIdAsync(id, theatreId);
}
