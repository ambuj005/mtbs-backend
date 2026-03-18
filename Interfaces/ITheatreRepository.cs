using MovieTicketBooking.Api.Models;
namespace MovieTicketBooking.Api.Interfaces;
public interface ITheatreRepository : IRepository<Theatre>
{
    Task<IEnumerable<Theatre>> GetByCityIdAsync(string cityId);
    Task<IEnumerable<Theatre>> GetByAreaIdAsync(string areaId);
}
