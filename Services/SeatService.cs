using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Services;

public class SeatService : ISeatService
{
    private readonly IShowRepository _showRepo;
    private readonly ISeatLockRepository _lockRepo; 
    private readonly IConfiguration _config;
    private readonly ILogger<SeatService> _logger;

    public SeatService(
        IShowRepository showRepo,
        ISeatLockRepository lockRepo, 
        IConfiguration config,
        ILogger<SeatService> logger)
    {
        _showRepo = showRepo;
        _lockRepo = lockRepo; 
        _config = config;
        _logger = logger;
    }

    public async Task<SeatLayoutResponse?> GetSeatLayoutAsync(string showId)
    { 
        var show = (await _showRepo.QueryAsync(q =>
            q.Where(s => s.Id == showId && s.IsActive))).FirstOrDefault();

        if (show == null || !show.Seats.Any())
            return null;

        var now = DateTime.UtcNow;
         
        var rows = show.Seats
            .GroupBy(s => new { s.Row, s.Category })
            .OrderBy(g => g.Key.Category)
            .ThenBy(g => g.Key.Row)
            .Select(g => new SeatRowDTO
            {
                RowLabel = g.Key.Row,
                Category = g.Key.Category.ToString(),
                Seats = g.OrderBy(s => s.Number).Select(s => new SeatDTO
                {
                    Id = s.Id,
                    ShowId = showId,
                    Row = s.Row,
                    Number = s.Number,
                    Category = s.Category.ToString(),
                    Status = s.Status == SeatStatus.Booked ? SeatStatus.Booked
                        : (s.Status == SeatStatus.Locked && s.LockedUntil > now) ? SeatStatus.Locked
                        : SeatStatus.Available,
                    Price = s.Price,
                    IsLocked = s.Status == SeatStatus.Locked && s.LockedUntil > now,
                    LockedUntil = s.LockedUntil,
                    LockSessionId = s.LockSessionId  
                }).ToList()
            })
            .ToList();

        return new SeatLayoutResponse
        {
            ShowId = show.Id,
            ScreenInfo = new ScreenInfo
            {
                ScreenNumber = show.ScreenNumber,
                ScreenType = show.ScreenType,
                TotalSeats = show.TotalSeats,
                AvailableSeats = show.AvailableSeats
            },
            Rows = rows,
            PriceCategories = show.PriceCategories
        };
    }
 
    public async Task<LockSeatsResponse> LockSeatsAsync(string showId, List<string> seatIds, string sessionId)
    {
        var show = (await _showRepo.QueryAsync(q =>
            q.Where(s => s.Id == showId && s.IsActive))).FirstOrDefault();

        if (show == null)
            return new LockSeatsResponse { Success = false, Message = "Show not found" };

        var now = DateTime.UtcNow;
         
        var requestedSeats = show.Seats.Where(s => seatIds.Contains(s.Id)).ToList();

        if (requestedSeats.Count != seatIds.Count)
            return new LockSeatsResponse { Success = false, Message = "Some seats not found" };
         
        var unavailable = requestedSeats
            .Where(s =>
                s.Status == SeatStatus.Booked ||
                (s.Status == SeatStatus.Locked &&
                 s.LockedUntil > now &&
                 s.LockSessionId != sessionId))
            .Select(s => s.Id)
            .ToList();

        if (unavailable.Any())
        {
            return new LockSeatsResponse
            {
                Success = false,
                Message = "Some seats are not available",
                UnavailableSeats = unavailable
            };
        }
         
        var lockDuration = _config.GetValue<int>("App:SeatLockDurationMinutes", 10);
        var expiresAt = now.AddMinutes(lockDuration);

        foreach (var seat in requestedSeats)
        {
            seat.Status = SeatStatus.Locked;
            seat.LockedUntil = expiresAt;
            seat.LockSessionId = sessionId;
        }
         
        await _showRepo.UpdateAsync(show);
         
        var seatLock = new SeatLock
        {
            ShowId = showId,
            SeatIds = seatIds,
            ExpiresAt = expiresAt,
            Status = SeatLockStatus.Active,
            TtlSeconds = lockDuration * 60,
            SessionId = sessionId
        };

        await _lockRepo.CreateAsync(seatLock);


        return new LockSeatsResponse
        {
            Success = true,
            LockId = seatLock.Id,
            LockedSeats = seatIds,
            ExpiresAt = expiresAt
        };
    }
     
    public async Task<bool> UnlockSeatsAsync(string lockId, string sessionId)
    {
        var locks = await _lockRepo.QueryAsync(q =>
            q.Where(l => l.Id == lockId && l.Status == SeatLockStatus.Active));

        var seatLock = locks.FirstOrDefault();
        if (seatLock == null || seatLock.SessionId != sessionId)
            return false;

        var show = (await _showRepo.QueryAsync(q =>
            q.Where(s => s.Id == seatLock.ShowId && s.IsActive))).FirstOrDefault();

        if (show == null) return false;
         
        var seatsToUnlock = show.Seats.Where(s =>
            seatLock.SeatIds.Contains(s.Id) &&
            s.LockSessionId == sessionId).ToList();

        foreach (var seat in seatsToUnlock)
        {
            seat.Status = SeatStatus.Available;
            seat.LockedUntil = null;
            seat.LockSessionId = null;
        }
         
        await _showRepo.UpdateAsync(show);

        seatLock.Status = SeatLockStatus.Released;
        await _lockRepo.UpdateAsync(seatLock);

        return true;
    }
     
    public async Task<bool> ExtendLockAsync(string lockId, string sessionId, int additionalMinutes = 5)
    {
        var locks = await _lockRepo.QueryAsync(q =>
            q.Where(l => l.Id == lockId && l.Status == SeatLockStatus.Active));

        var seatLock = locks.FirstOrDefault();
        if (seatLock == null || seatLock.SessionId != sessionId || seatLock.ExpiresAt < DateTime.UtcNow)
            return false;

        var show = (await _showRepo.QueryAsync(q =>
            q.Where(s => s.Id == seatLock.ShowId && s.IsActive))).FirstOrDefault();

        if (show == null) return false;

        var newExpiry = DateTime.UtcNow.AddMinutes(additionalMinutes);
        seatLock.ExpiresAt = newExpiry;

        var seatsToExtend = show.Seats.Where(s =>
            seatLock.SeatIds.Contains(s.Id) &&
            s.LockSessionId == sessionId).ToList();

        foreach (var seat in seatsToExtend)
            seat.LockedUntil = newExpiry;

        await _showRepo.UpdateAsync(show);
        await _lockRepo.UpdateAsync(seatLock);

        return true;
    }
    public async Task ProcessExpiredLocksAsync()
    {
        var expired = await _lockRepo.GetExpiredLocksAsync();

        foreach (var seatLock in expired)
        {
            try
            {
                var show = (await _showRepo.QueryAsync(q =>
                    q.Where(s => s.Id == seatLock.ShowId && s.IsActive))).FirstOrDefault();

                if (show == null) continue;

                var seatsToUnlock = show.Seats.Where(s =>
                    seatLock.SeatIds.Contains(s.Id) &&
                    s.Status == SeatStatus.Locked).ToList();

                foreach (var seat in seatsToUnlock)
                {
                    seat.Status = SeatStatus.Available;
                    seat.LockedUntil = null;
                    seat.LockSessionId = null;
                }

                await _showRepo.UpdateAsync(show);

                seatLock.Status = SeatLockStatus.Expired;
                await _lockRepo.UpdateAsync(seatLock);

                _logger.LogInformation("Expired lock {LockId} processed", seatLock.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process expired lock {LockId}", seatLock.Id);
            }
        }
    }
}