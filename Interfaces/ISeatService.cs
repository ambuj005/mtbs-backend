using MovieTicketBooking.Api.DTO;

namespace MovieTicketBooking.Api.Interfaces;

public interface ISeatService
{
    Task<LockSeatsResponse> LockSeatsAsync(string showId, List<string> seatIds, string sessionId);
    Task<bool> UnlockSeatsAsync(string lockId, string sessionId);
    Task<bool> ExtendLockAsync(string lockId, string sessionId, int additionalMinutes = 5); 
    Task ProcessExpiredLocksAsync();
    Task<SeatLayoutResponse?> GetSeatLayoutAsync(string showId);

}
