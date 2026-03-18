using MovieTicketBooking.Api.Data;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Repo;

public class ShowRepository : BaseRepository<Show>, IShowRepository
{
    public ShowRepository(MongoDbContext db, ILogger<ShowRepository> log) : base(db.Shows, log) { }

    public async Task<IEnumerable<Show>> GetByMovieIdAsync(
        string movieId,
        DateTime? date = null)
    {
        if (date.HasValue)
        {
            var start = date.Value.Date;
            var end = start.AddDays(1);

            var shows = await QueryAsync(
                q => q.Where(s =>
                    s.IsActive &&
                    s.MovieId == movieId &&
                    s.ShowTime >= start &&
                    s.ShowTime < end
                )
            );

            return shows.Where(s => s.Status == ShowStatus.Scheduled);
        }

        var allShows = await QueryAsync(
            q => q.Where(s =>
                s.IsActive &&
                s.MovieId == movieId
            )
        );

        return allShows.Where(s => s.Status == ShowStatus.Scheduled);
    }

    public async Task<IEnumerable<Show>> GetByTheatreIdAllAsync(string theatreId)
    {
        return await QueryAsync(
            q => q.Where(s =>
                s.IsActive &&
                s.TheatreId == theatreId),
            theatreId
        );
    }

    public async Task<IEnumerable<Show>> GetByTheatreIdAsync(
        string theatreId,
        DateTime? date = null)
    {
        if (date.HasValue)
        {
            var start = date.Value.Date;
            var end = start.AddDays(1);

            var shows = await QueryAsync(
                q => q.Where(s =>
                    s.IsActive &&
                    s.TheatreId == theatreId &&
                    s.ShowTime >= start &&
                    s.ShowTime < end
                ),
                theatreId
            );

            return shows.Where(s => s.Status == ShowStatus.Scheduled);
        }

        var allShows = await QueryAsync(
            q => q.Where(s =>
                s.IsActive &&
                s.TheatreId == theatreId
            ),
            theatreId
        );

        return allShows.Where(s => s.Status == ShowStatus.Scheduled);
    }

    public async Task<IEnumerable<Show>> GetByMovieAndTheatreAllAsync(
        string movieId,
        string theatreId)
    {
        return await QueryAsync(
            q => q.Where(s =>
                s.IsActive &&
                s.MovieId == movieId &&
                s.TheatreId == theatreId
            ),
            theatreId
        );
    }

    public async Task<IEnumerable<Show>> GetByMovieAndTheatreAsync(
        string movieId,
        string theatreId,
        DateTime? date = null)
    {
        var start = date?.Date ?? DateTime.UtcNow.Date;
        var end = start.AddDays(1);

        Console.WriteLine($"[ShowRepo.GetByMovieAndTheatreAsync]");
        Console.WriteLine($"  movieId: {movieId}");
        Console.WriteLine($"  theatreId: {theatreId}");
        Console.WriteLine($"  date: {date}");
        Console.WriteLine($"  start: {start:O}");
        Console.WriteLine($"  end: {end:O}");

        var shows = await QueryAsync(
            q => q.Where(s =>
                s.IsActive &&
                s.MovieId == movieId &&
                s.TheatreId == theatreId &&
                s.ShowTime >= start &&
                s.ShowTime < end
            ),
            theatreId
        );

        Console.WriteLine($"  shows before status filter: {shows.Count()}");
         
        var filtered = shows.Where(s => s.Status == ShowStatus.Scheduled).ToList();

        Console.WriteLine($"  shows after status filter: {filtered.Count}");

        return filtered;
    }

    public async Task<IEnumerable<Show>> GetUpcomingByTheatreIdAsync(
        string theatreId,
        DateTime? from = null,
        int days = 30)
    {
        var start = from?.Date ?? DateTime.UtcNow.Date;
        var end = start.AddDays(days);

        var shows = await QueryAsync(
            q => q.Where(s =>
                s.IsActive &&
                s.TheatreId == theatreId &&
                s.ShowTime >= start &&
                s.ShowTime < end),
            theatreId
        );

        return shows.Where(s => s.Status == ShowStatus.Scheduled);
    }
}