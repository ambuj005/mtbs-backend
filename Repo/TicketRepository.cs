using MovieTicketBooking.Api.Data;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Repo;

public class TicketRepository : BaseRepository<Ticket>, ITicketRepository
{
    public TicketRepository(MongoDbContext db, ILogger<TicketRepository> log)
        : base(db.Tickets, log) { }

    public async Task<IEnumerable<Ticket>> GetAllAsync()
        => await QueryAsync(q => q.OrderByDescending(t => t.IssuedAt));

    public async Task<Ticket?> GetByBookingIdAsync(string bookingId)
        => (await QueryAsync(q => q.Where(t => t.BookingId == bookingId))).FirstOrDefault();

    public async Task<Ticket?> GetByQrDataAsync(string qrData)
        => (await QueryAsync(q => q.Where(t => t.QrData == qrData))).FirstOrDefault();

    public async Task<Ticket?> GetByIdOrTicketNumberAsync(string lookupValue)
    {
        // Search by QRData(TicketId) OR TicketNumber
        var tickets = await QueryAsync(q => q.Where(t =>
            t.Id == lookupValue ||
            t.TicketNumber == lookupValue));

        return tickets.FirstOrDefault();
    }
}
