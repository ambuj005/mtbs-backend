using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;
using MovieTicketBooking.Api.Utils;
using System;

namespace MovieTicketBooking.Api.Services;

public class PaymentService : IPaymentService
{
    private readonly IBookingService _bookingService;
    private readonly IBookingRepository _bookingRepo;
    private readonly ITicketService _ticketService; 
    private readonly ISeatLockRepository _lockRepo;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentService> _logger;
    private readonly IShowRepository _showRepo;
    private readonly IMessagePublisher _messagePublisher;

    public PaymentService(
        IBookingService bookingService,
        IBookingRepository bookingRepo,
        ITicketService ticketService, 
        IMessagePublisher messagePublisher,
        ISeatLockRepository lockRepo,
        IShowRepository showRepo,             
        IConfiguration configuration,
        ILogger<PaymentService> logger)
    {
        _bookingService = bookingService;
        _bookingRepo = bookingRepo;
        _ticketService = ticketService;
        _messagePublisher = messagePublisher;
        _lockRepo = lockRepo;
        _configuration = configuration;
        _logger = logger;
        _showRepo = showRepo;
    }

    public async Task<PaymentConfirmationResponse> ProcessPaymentWithBookingAsync(
        CreateBookingRequest bookingDetails,
        string razorpayPaymentId,
        string razorpayOrderId,
        string razorpaySignature)
    {
        try
        {
            var keySecret = _configuration["Razorpay:KeySecret"];
            if (string.IsNullOrWhiteSpace(keySecret))
                throw new InvalidOperationException("Razorpay secret not configured");

            var locks = await _lockRepo.QueryAsync(q =>
                q.Where(l => l.Id == bookingDetails.SeatLockId));

            var seatLock = locks.FirstOrDefault();

            if (seatLock == null || string.IsNullOrWhiteSpace(seatLock.RazorpayOrderId))
                throw new InvalidOperationException("Original Razorpay order not found");

            var isValidSignature = RazorpaySignatureVerifier.Verify(
                seatLock.RazorpayOrderId,
                razorpayPaymentId,
                razorpaySignature,
                keySecret
            );

            if (!isValidSignature)
                throw new InvalidOperationException("Invalid payment signature");

            // Create booking
            var bookingResponse = await _bookingService.CreateBookingAsync(bookingDetails);

            var booking = await _bookingRepo.GetByIdCrossPartitionAsync(bookingResponse.BookingId)
                ?? throw new InvalidOperationException("Booking creation failed");

            booking.PaymentId = razorpayPaymentId;
            booking.OrderId = seatLock.RazorpayOrderId;
            booking.Status = BookingStatus.Confirmed;
            booking.ConfirmedAt = DateTime.UtcNow;

            await _bookingRepo.UpdateAsync(booking);

            //MARK SEATS AS BOOKED  
            var show = await _showRepo.GetByIdAsync(booking.ShowId, booking.TheatreId)
                ?? throw new InvalidOperationException("Show not found");

            foreach (var seat in show.Seats)
            {
                if (booking.Seats.Any(s => s.SeatId == seat.Id))
                {
                    seat.Status = SeatStatus.Booked;
                }
            }

            await _showRepo.UpdateAsync(show);

            // Release seat lock
            await _lockRepo.DeleteAsync(seatLock.Id, seatLock.PartitionKey);

            // Generate ticket
            var ticket = await _ticketService.GenerateTicketAsync(booking);

            await _messagePublisher.PublishTicketEmailAsync(ticket);

            return new PaymentConfirmationResponse
            {
                Success = true,
                BookingId = booking.Id,
                OrderId = booking.OrderId,
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                Message = "Payment verified and successful! Ticket sent to your email."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment processing failed");
            throw;
        }
    }

    public async Task<PaymentConfirmationResponse> ProcessMockPaymentAsync(
        CreateBookingRequest bookingDetails,
        string razorpayPaymentId,
        string razorpayOrderId)
    {
        var bookingResponse = await _bookingService.CreateBookingAsync(bookingDetails);

        var booking = await _bookingRepo.GetByIdCrossPartitionAsync(bookingResponse.BookingId)
            ?? throw new InvalidOperationException("Booking creation failed");

        booking.PaymentId = razorpayPaymentId;
        booking.OrderId = razorpayOrderId;
        booking.Status = BookingStatus.Confirmed;
        booking.ConfirmedAt = DateTime.UtcNow;
        booking.PaymentMethod = PaymentMethod.Card;

        await _bookingRepo.UpdateAsync(booking);

        // MARK SEATS AS BOOKED
        var show = await _showRepo.GetByIdAsync(booking.ShowId, booking.TheatreId)
            ?? throw new InvalidOperationException("Show not found");

        foreach (var seat in show.Seats)
        {
            if (booking.Seats.Any(s => s.SeatId == seat.Id))
            {
                seat.Status = SeatStatus.Booked;
            }
        }

        await _showRepo.UpdateAsync(show);

        // Release seat lock
        if (!string.IsNullOrWhiteSpace(bookingDetails.SeatLockId))
        {
            var seatLock = await _lockRepo.GetByIdAsync(
                bookingDetails.SeatLockId,
                bookingDetails.ShowId
            );

            if (seatLock != null)
            {
                await _lockRepo.DeleteAsync(seatLock.Id, seatLock.PartitionKey);
            }
        }

        var ticket = await _ticketService.GenerateTicketAsync(booking); 

        return new PaymentConfirmationResponse
        {
            Success = true,
            BookingId = booking.Id,
            OrderId = booking.OrderId,
            TicketId = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            Message = "Mock payment successful! Ticket generated."
        };
    }

    public Task<PaymentResponse> GetPaymentStatusAsync(string paymentId)
        => Task.FromResult(new PaymentResponse
        {
            PaymentId = paymentId,
            Status = PaymentStatus.Success,
            Timestamp = DateTime.UtcNow
        });
}
