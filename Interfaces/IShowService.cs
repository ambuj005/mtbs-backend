using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Models;
namespace MovieTicketBooking.Api.Interfaces;
public interface IShowService
{
    Task<IEnumerable<Show>> GetShowsAsync(string? movieId = null, string? theatreId = null, DateTime? date = null);
    Task<Show?> GetShowByIdAsync(string id, string theatreId); 
}
