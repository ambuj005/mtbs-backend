using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Models;
namespace MovieTicketBooking.Api.Interfaces;
public interface IMovieRepository : IRepository<Movie>
{
    Task<IEnumerable<Movie>> SearchAsync(string query);
    Task<PagedResult<Movie>> GetByIdsPagedAsync(
    IReadOnlyCollection<string> movieIds,
    int page,
    int pageSize,
    string? search,
    string? genre,
    string? language
);
}
