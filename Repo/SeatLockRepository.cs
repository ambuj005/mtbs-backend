using MovieTicketBooking.Api.Data;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;
using MovieTicketBooking.Api.Repo;

public class SeatLockRepository : BaseRepository<SeatLock>, ISeatLockRepository
{
    public SeatLockRepository(
        MongoDbContext db,
        ILogger<SeatLockRepository> log)
        : base(db.SeatLocks, log) { }

    public async Task<IEnumerable<SeatLock>> GetExpiredLocksAsync()
        => await QueryAsync(q =>
            q.Where(l =>
                l.Status == SeatLockStatus.Active &&
                l.ExpiresAt <= DateTime.UtcNow));
}
