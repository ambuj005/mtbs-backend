using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;
using System.Text.Json;

namespace MovieTicketBooking.Api.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepo;
    private readonly IMovieRepository _movieRepo;
    private readonly ITheatreRepository _theatreRepo;
    private readonly IShowRepository _showRepo;
    private readonly ICityRepository _cityRepo;
    private readonly IQRCodeService _qrService;
    private readonly ILogger<TicketService> _logger;

    public TicketService(
        ITicketRepository ticketRepo,
        IMovieRepository movieRepo,
        ITheatreRepository theatreRepo,
        IShowRepository showRepo,
        ICityRepository cityRepo,
        IQRCodeService qrService,
        ILogger<TicketService> logger)
    {
        _ticketRepo = ticketRepo;
        _movieRepo = movieRepo;
        _theatreRepo = theatreRepo;
        _showRepo = showRepo;
        _cityRepo = cityRepo;
        _qrService = qrService;
        _logger = logger;
    }

    public async Task<Ticket> GenerateTicketAsync(Booking booking)
    {
        // Fetch related data
        var movie = (await _movieRepo.QueryAsync(q => q.Where(m => m.Id == booking.MovieId)))
            .FirstOrDefault();

        var theatre = await _theatreRepo.GetByIdAsync(
            booking.TheatreId,
            booking.TheatreId);

        var show = await _showRepo.GetByIdAsync(
            booking.ShowId,
            booking.TheatreId);

        var city = theatre != null
            ? await _cityRepo.GetCityByIdAsync(theatre.CityId)
            : null;

        var ticketId = Guid.NewGuid().ToString();

        var ticket = new Ticket
        {
            Id = ticketId,

            BookingId = booking.Id,
            OrderId = booking.OrderId,

            UserEmail = booking.UserEmail,
            UserName = booking.UserName,

            QrData = ticketId,
            QrCode = _qrService.GenerateQRCode(ticketId),

            Movie = new TicketMovieInfo
            {
                Id = movie?.Id ?? booking.MovieId,
                Title = movie?.Title ?? booking.MovieTitle,
                PosterUrl = movie?.PosterUrl ?? "",
                Duration = movie?.Duration ?? 0,
                Language = movie?.Language ?? "",
                Rating = movie?.Rating ?? ""
            },

            Theatre = new TicketTheatreInfo
            {
                Id = theatre?.Id ?? booking.TheatreId,
                Name = theatre?.Name ?? booking.TheatreName,
                Address = theatre?.Address ?? "",
                City = city?.Name ?? ""
            },

            Show = new TicketShowInfo
            {
                Id = show?.Id ?? booking.ShowId,
                ShowTime = show?.ShowTime ?? booking.ShowTime,
                ScreenNumber = show?.ScreenNumber ?? booking.ScreenNumber,
                ScreenType = show?.ScreenType ?? "2D"
            },

            Seats = booking.Seats,
            TotalAmount = booking.TotalAmount,
            Status = TicketStatus.Valid,
            ValidUntil = show?.EndTime ?? booking.ShowTime.AddHours(3)
        };

        await _ticketRepo.CreateAsync(ticket);

        _logger.LogInformation(
            "Generated ticket {TicketNumber} for booking {BookingId}",
            ticket.TicketNumber,
            booking.Id);

        return ticket;
    }

    public async Task<Ticket?> GetTicketByIdAsync(string ticketId)
    {
        var tickets = await _ticketRepo.QueryAsync(q => q.Where(t => t.Id == ticketId));
        return tickets.FirstOrDefault();
    }

    public Task<Ticket?> GetTicketByBookingAsync(string bookingId)
        => _ticketRepo.GetByBookingIdAsync(bookingId);

    public Task<IEnumerable<Ticket>> GetAllTicketsAsync()
        => _ticketRepo.GetAllAsync();

    public async Task<VerifyQRResponse> VerifyQRCodeAsync(VerifyQRRequest req)
    {
        var lookupValue = !string.IsNullOrEmpty(req.TicketId)
            ? req.TicketId
            : req.QrData;

        if (string.IsNullOrEmpty(lookupValue))
        {
            return new VerifyQRResponse
            {
                IsValid = false,
                Message = "Ticket ID or Ticket Number is required"
            };
        }

        _logger.LogInformation("Looking up ticket with value: {LookupValue}", lookupValue);

        // This method handles both ID and TicketNumber lookup
        var ticket = await _ticketRepo.GetByIdOrTicketNumberAsync(lookupValue);

        if (ticket == null)
        {
            _logger.LogWarning("Ticket not found with lookup value: {LookupValue}", lookupValue);
            return new VerifyQRResponse
            {
                IsValid = false,
                Message = "Invalid ticket - ticket not found"
            };
        }

        var errors = new List<string>();
        var utcNow = DateTime.UtcNow;

        // Check if ticket is for today's show
        if (ticket.Show.ShowTime.Date != utcNow.Date)
        {
            errors.Add(
                $"This ticket is for {ticket.Show.ShowTime:yyyy-MM-dd}, not today ({utcNow:yyyy-MM-dd})"
            );
        }

        // Check if ticket belongs to correct theatre
        if (!string.IsNullOrEmpty(req.TheatreId) && ticket.Theatre.Id != req.TheatreId)
        {
            errors.Add($"This ticket is for {ticket.Theatre.Name}, not this theatre");
        }

        // Check if ticket is already used
        if (ticket.Status == TicketStatus.Used)
        {
            errors.Add($"Ticket already used at {ticket.UsedAt:h}");
        }

        // Check if ticket is cancelled
        if (ticket.Status == TicketStatus.Cancelled)
        {
            errors.Add("Ticket has been cancelled");
        }

        if (ticket.ValidUntil < utcNow)
        {
            if (ticket.Status == TicketStatus.Valid)
            {
                ticket.Status = TicketStatus.Expired;
                await _ticketRepo.UpdateAsync(ticket);
            }
            errors.Add("Show has ended - ticket expired");
        }

        //Check entry time window (30 minutes before show time)
         
        var entryWindowMinutes = 30;
        var earliestEntryUtc = ticket.Show.ShowTime.AddMinutes(-entryWindowMinutes);

        if (utcNow < earliestEntryUtc)
        {
            errors.Add(
                $"Entry not open yet. Entry opens at {earliestEntryUtc:HH:mm} IST (30 minutes before show time)"
            );
        }

        if (errors.Any())
        {
            return new VerifyQRResponse
            {
                IsValid = false,
                Message = string.Join("; ", errors),
                ValidationErrors = errors,
                Ticket = MapToResponse(ticket)
            };
        }

        // All validations passed - mark ticket as used if requested
        if (req.MarkAsUsed)
        {
            ticket.Status = TicketStatus.Used;
            ticket.UsedAt = utcNow; 
            ticket.VerifiedBy = req.VerifiedBy;
            await _ticketRepo.UpdateAsync(ticket);

            _logger.LogInformation(
                "Ticket {TicketNumber} verified and marked as used by {VerifiedBy}",
                ticket.TicketNumber,
                req.VerifiedBy);
        }

        return new VerifyQRResponse
        {
            IsValid = true,
            Message = "✅ Valid ticket - Entry allowed",
            Ticket = MapToResponse(ticket),
            VerifiedAt = utcNow  // ← Change here
        };
    }
    private static TicketResponse MapToResponse(Ticket t) => new()
    {
        TicketId = t.Id,
        TicketNumber = t.TicketNumber,
        BookingId = t.BookingId,
        OrderId = t.OrderId,
        QrCode = t.QrCode,
        Movie = t.Movie,
        Theatre = t.Theatre,
        Show = t.Show,
        Seats = t.Seats,
        TotalAmount = t.TotalAmount,
        Status = t.Status,
        IssuedAt = t.IssuedAt,
        ValidUntil = t.ValidUntil
    };
}
