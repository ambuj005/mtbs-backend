using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Interfaces;

public interface IBookingRepository : IRepository<Booking>
{
    Task<Booking?> GetByIdCrossPartitionAsync(string bookingId);
    Task<IEnumerable<Booking>> GetAllAsync();
}
