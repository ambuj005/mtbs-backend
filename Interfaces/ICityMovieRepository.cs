using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Interfaces
{
    public interface ICityMovieRepository
    {
        Task<PagedResult<CityMovie>> GetByCityPagedAsync(
            string cityId,
            int page,
            int pageSize);
    }

}
