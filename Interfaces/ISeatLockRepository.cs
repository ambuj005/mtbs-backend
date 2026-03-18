using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Interfaces;

public interface ISeatLockRepository : IRepository<SeatLock>
{
    Task<IEnumerable<SeatLock>> GetExpiredLocksAsync();
}
