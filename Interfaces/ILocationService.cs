using MovieTicketBooking.Api.Models;
namespace MovieTicketBooking.Api.Interfaces;
public interface ILocationService
{
    Task<IEnumerable<City>> GetCitiesAsync();
    Task<IEnumerable<City>> GetPopularCitiesAsync();
    Task<City?> GetCityByIdAsync(string id); 
}
