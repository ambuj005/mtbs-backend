using MovieTicketBooking.Api.Data;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Repo;

public class CityRepository : BaseRepository<City>, ICityRepository
{
    public CityRepository(MongoDbContext db, ILogger<CityRepository> log) : base(db.Cities, log) { }
    public override async Task<IEnumerable<City>> GetAllAsync(string? partitionKey = null)
        => await QueryAsync(q => q.OrderBy(c => c.Name), partitionKey);

    public async Task<IEnumerable<City>> GetPopularCitiesAsync()
        => await QueryAsync(q => q.Where(c => c.IsPopular).OrderBy(c => c.Name));

    public async Task<City?> GetCityByIdAsync(string id)
        => (await QueryAsync(q => q.Where(c => c.Id == id))).FirstOrDefault();
}
