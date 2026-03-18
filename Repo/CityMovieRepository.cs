using MovieTicketBooking.Api.Data;
using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MovieTicketBooking.Api.Repo
{
    public class CityMovieRepository
        : BaseRepository<CityMovie>, ICityMovieRepository
    {
        public CityMovieRepository(
            MongoDbContext db,
            ILogger<CityMovieRepository> log)
            : base(db.CityMovies, log) { }

        public async Task<PagedResult<CityMovie>> GetByCityPagedAsync(
            string cityId,
            int page,
            int pageSize)
        {
            var query = _collection
                .AsQueryable()
                .Where(x => x.IsActive && x.CityId == cityId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.MovieId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<CityMovie>
            {
                Items = items,
                TotalCount = totalCount
            };
        }
    }

}
