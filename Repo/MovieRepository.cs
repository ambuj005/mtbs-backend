using MovieTicketBooking.Api.Data;
using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MovieTicketBooking.Api.Repo;

public class MovieRepository : BaseRepository<Movie>, IMovieRepository
{
    public MovieRepository(MongoDbContext db, ILogger<MovieRepository> log) : base(db.Movies, log) { }
    public async Task<IEnumerable<Movie>> SearchAsync(string query)
        => await QueryAsync(q =>
            q.Where(m => m.Title.ToLower().Contains(query.ToLower())));

    public async Task<PagedResult<Movie>> GetByIdsPagedAsync(
        IReadOnlyCollection<string> movieIds,
        int page,
        int pageSize,
        string? search,
        string? genre,
        string? language)
    {
        var query = _collection
            .AsQueryable()
            .Where(m => m.IsActive && movieIds.Contains(m.Id));
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m =>
                m.Title.ToLower().Contains(search.ToLower()));

        if (!string.IsNullOrWhiteSpace(language))
            query = query.Where(m => m.Language == language);

        if (!string.IsNullOrWhiteSpace(genre))
            query = query.Where(m => m.Genre.Contains(genre));

        query = query.OrderByDescending(m => m.ReleaseDate);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Movie>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

}