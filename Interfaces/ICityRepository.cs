using MovieTicketBooking.Api.Models;
namespace MovieTicketBooking.Api.Interfaces;
public interface ICityRepository : IRepository<City>
{
    Task<IEnumerable<City>> GetPopularCitiesAsync();
    Task<City?> GetCityByIdAsync(string id);
}
