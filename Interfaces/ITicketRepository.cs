using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Interfaces;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<Ticket?> GetByBookingIdAsync(string bookingId);
    Task<Ticket?> GetByQrDataAsync(string qrData);
    Task<IEnumerable<Ticket>> GetAllAsync();
    Task<Ticket?> GetByIdOrTicketNumberAsync(string lookupValue);
}
