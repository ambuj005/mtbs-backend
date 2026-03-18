using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly ISeatLockRepository _lockRepo;
    private readonly IShowRepository _showRepo;
    private readonly IMovieRepository _movieRepo;
    private readonly ITheatreRepository _theatreRepo;
    private readonly ILogger<BookingService> _logger;

    private const decimal ConvenienceFeePercent = 0.03m;
    private const decimal GstPercent = 0.18m;

    public BookingService(
        IBookingRepository bookingRepo,
        ISeatLockRepository lockRepo,
        IShowRepository showRepo,
        IMovieRepository movieRepo,
        ITheatreRepository theatreRepo,
        ILogger<BookingService> logger)
    {
        _bookingRepo = bookingRepo;
        _lockRepo = lockRepo;
        _showRepo = showRepo;
        _movieRepo = movieRepo;
        _theatreRepo = theatreRepo;
        _logger = logger;
    }

    public async Task<BookingResponse> CreateBookingAsync(CreateBookingRequest req)
    {
        var locks = await _lockRepo.QueryAsync(q =>
            q.Where(l => l.Id == req.SeatLockId && l.Status == SeatLockStatus.Active));

        var seatLock = locks.FirstOrDefault();
        if (seatLock == null || seatLock.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Seat lock expired or invalid");

        if (seatLock.ShowId != req.ShowId)
            throw new InvalidOperationException("Seat lock does not belong to this show");

        var lockedSeatIds = seatLock.SeatIds.OrderBy(id => id).ToList();
        var requestedSeatIds = req.SeatIds.OrderBy(id => id).ToList();

        if (!lockedSeatIds.SequenceEqual(requestedSeatIds))
            throw new InvalidOperationException("Seat selection does not match locked seats");

        var show = (await _showRepo.QueryAsync(q =>
            q.Where(s => s.Id == req.ShowId && s.IsActive))).FirstOrDefault();

        if (show == null)
            throw new InvalidOperationException("Show not found");

        var lockedSeats = show.Seats.Where(s =>
            req.SeatIds.Contains(s.Id) &&
            s.Status == SeatStatus.Locked).ToList();

        if (lockedSeats.Count != req.SeatIds.Count)
            throw new InvalidOperationException("Some seats are not locked");

        var movie = (await _movieRepo.QueryAsync(q =>
            q.Where(m => m.Id == req.MovieId)
        )).FirstOrDefault();
         
        var theatre = (await _theatreRepo.QueryAsync(q =>
            q.Where(t => t.Id == req.TheatreId)
        )).FirstOrDefault();
         
        _logger.LogInformation(
            "Booking validation: MovieFound={MovieFound}, TheatreFound={TheatreFound}, MovieId={MovieId}, TheatreId={TheatreId}",
            movie != null,
            theatre != null,
            req.MovieId,
            req.TheatreId
        );

        if (movie == null || theatre == null)
            throw new InvalidOperationException("Invalid movie or theatre");

        var baseAmt = lockedSeats.Sum(s => s.Price);
        var fee = Math.Round(baseAmt * ConvenienceFeePercent, 2);
        var taxes = Math.Round(fee * GstPercent, 2);
        var total = baseAmt + fee + taxes;

        var booking = new Booking
        {
            UserEmail = req.UserEmail,
            UserName = req.UserName,
            UserPhone = req.UserPhone,

            MovieId = req.MovieId,
            MovieTitle = movie.Title,
            TheatreId = req.TheatreId,
            TheatreName = theatre.Name,
            ShowId = req.ShowId,
            ShowTime = show.ShowTime,
            ScreenNumber = show.ScreenNumber,

            Seats = lockedSeats.Select(s => new BookedSeat
            {
                SeatId = s.Id,
                Row = s.Row,
                Number = s.Number,
                Category = s.Category,
                Price = s.Price
            }).ToList(),

            BaseAmount = baseAmt,
            ConvenienceFee = fee,
            Taxes = taxes,
            TotalAmount = total,
            SeatLockId = req.SeatLockId,
            Status = BookingStatus.Pending
        };

        await _bookingRepo.CreateAsync(booking);

        _logger.LogInformation("Created booking {BookingId}", booking.Id);

        return new BookingResponse
        {
            BookingId = booking.Id,
            OrderId = booking.OrderId,
            Status = booking.Status,
            MovieTitle = booking.MovieTitle,
            TheatreName = booking.TheatreName,
            ShowTime = booking.ShowTime,
            ScreenNumber = booking.ScreenNumber,
            Seats = booking.Seats,
            BaseAmount = baseAmt,
            ConvenienceFee = fee,
            Taxes = taxes,
            TotalAmount = total,
            CreatedAt = booking.CreatedAt
        };
    }

    public Task<Booking?> GetBookingByIdAsync(string bookingId)
        => _bookingRepo.GetByIdCrossPartitionAsync(bookingId);

    public Task<IEnumerable<Booking>> GetAllBookingsAsync()
        => _bookingRepo.GetAllAsync();

    public async Task<bool> ConfirmBookingAsync(string bookingId, string paymentId, PaymentMethod paymentMethod)
    {
        var booking = await _bookingRepo.GetByIdCrossPartitionAsync(bookingId);
        if (booking == null || booking.Status != BookingStatus.Pending)
            return false;

        booking.Status = BookingStatus.Confirmed;
        booking.PaymentId = paymentId;
        booking.PaymentMethod = paymentMethod;
        booking.ConfirmedAt = DateTime.UtcNow;

        var show = (await _showRepo.QueryAsync(q =>
            q.Where(s => s.Id == booking.ShowId && s.IsActive))).FirstOrDefault();

        if (show == null) return false;

        var seatsToBook = show.Seats.Where(s =>
            booking.Seats.Select(bs => bs.SeatId).Contains(s.Id)).ToList();

        foreach (var seat in seatsToBook)
        {
            seat.Status = SeatStatus.Booked;
            seat.BookingId = booking.Id;
            seat.LockedUntil = null;
            seat.LockSessionId = null;
        }

        show.AvailableSeats -= booking.Seats.Count;

        await _showRepo.UpdateAsync(show);

        if (!string.IsNullOrEmpty(booking.SeatLockId))
        {
            var locks = await _lockRepo.QueryAsync(q => q.Where(l => l.Id == booking.SeatLockId));
            var seatLock = locks.FirstOrDefault();
            if (seatLock != null)
            {
                seatLock.Status = SeatLockStatus.Converted;
                await _lockRepo.UpdateAsync(seatLock);
            }
        }

        await ((IRepository<Booking>)_bookingRepo).UpdateAsync(booking);

        _logger.LogInformation("Booking {BookingId} confirmed with payment {PaymentId}", bookingId, paymentId);

        return true;
    }

    public async Task<bool> CancelBookingAsync(string bookingId, string? reason = null)
    {
        var booking = await _bookingRepo.GetByIdCrossPartitionAsync(bookingId);
        if (booking == null || booking.Status != BookingStatus.Confirmed)
            return false;

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = DateTime.UtcNow;
        booking.CancellationReason = reason;

        var show = (await _showRepo.QueryAsync(q =>
            q.Where(s => s.Id == booking.ShowId && s.IsActive))).FirstOrDefault();

        if (show == null) return false;

        var seatsToRelease = show.Seats.Where(s =>
            booking.Seats.Select(bs => bs.SeatId).Contains(s.Id)).ToList();

        foreach (var seat in seatsToRelease)
        {
            seat.Status = SeatStatus.Available;
            seat.BookingId = null;
        }

        show.AvailableSeats += booking.Seats.Count;

        await _showRepo.UpdateAsync(show);

        await _bookingRepo.UpdateAsync(booking);

        _logger.LogInformation("Booking {BookingId} cancelled", bookingId);

        return true;
    }
}