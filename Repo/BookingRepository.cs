using MovieTicketBooking.Api.Data;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Repo;

public class BookingRepository : BaseRepository<Booking>, IBookingRepository
{
    public BookingRepository(MongoDbContext db, ILogger<BookingRepository> log)
        : base(db.Bookings, log) { }

    public async Task<Booking?> GetByIdCrossPartitionAsync(string bookingId)
    {
        return (await QueryAsync(q => q.Where(b => b.Id == bookingId)))
            .FirstOrDefault();
    }

    public async Task<IEnumerable<Booking>> GetAllAsync()
        => await QueryAsync(q => q.OrderByDescending(b => b.CreatedAt));
}
