using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Models;
namespace MovieTicketBooking.Api.Interfaces;
public interface ITheatreService
{
    Task<IEnumerable<Theatre>> GetTheatresAsync(string? cityId = null, string? areaId = null);
    Task<Theatre?> GetTheatreByIdAsync(string id, string cityId);
    Task<IEnumerable<ShowWithTheatreDTO>> GetTheatresWithShowsAsync(string movieId, string cityId, DateTime? date = null);
}
