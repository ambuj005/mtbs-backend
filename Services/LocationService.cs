using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Services;

public class LocationService : ILocationService
{
    private readonly ICityRepository _cityRepo; 
    public LocationService(ICityRepository cityRepo) { _cityRepo = cityRepo;}

    public async Task<IEnumerable<City>> GetCitiesAsync() => (await _cityRepo.GetAllAsync()).OrderBy(c => c.Name);
    public Task<IEnumerable<City>> GetPopularCitiesAsync() => _cityRepo.GetPopularCitiesAsync();
    public Task<City?> GetCityByIdAsync(string id) => _cityRepo.GetCityByIdAsync(id); 
}
