using MovieTicketBooking.Api.Data;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Repo;

public class TheatreRepository : BaseRepository<Theatre>, ITheatreRepository
{
    public TheatreRepository(MongoDbContext db, ILogger<TheatreRepository> log)
        : base(db.Theatres, log) { }

    public async Task<IEnumerable<Theatre>> GetByCityIdAsync(string cityId)
    {
         return await QueryAsync(q =>
            q.Where(t => t.IsActive && t.CityId == cityId)
             .OrderBy(t => t.Name)
        );
    }

    public async Task<IEnumerable<Theatre>> GetByAreaIdAsync(string areaId)
    { 
        return await QueryAsync(q =>
            q.Where(t => t.IsActive && t.AreaId == areaId)
             .OrderBy(t => t.Name)
        );
    }
}
