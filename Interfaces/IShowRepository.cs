using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Interfaces;

public interface IShowRepository : IRepository<Show>
{ 
    Task<IEnumerable<Show>> GetByMovieIdAsync(string movieId, DateTime? date = null);
    Task<IEnumerable<Show>> GetByTheatreIdAsync(string theatreId, DateTime? date = null);
    Task<IEnumerable<Show>> GetByMovieAndTheatreAsync(
        string movieId,
        string theatreId,
        DateTime? date = null
    );
    Task<IEnumerable<Show>> GetByMovieAndTheatreAllAsync(
    string movieId,
    string theatreId
);
    Task<IEnumerable<Show>> GetUpcomingByTheatreIdAsync(
        string theatreId,
        DateTime? from = null,
        int days = 30
    );
     
    Task<IEnumerable<Show>> GetByTheatreIdAllAsync(string theatreId);
}
