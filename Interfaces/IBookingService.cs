using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Interfaces;

public interface IBookingService
{
    Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request);
    Task<Booking?> GetBookingByIdAsync(string bookingId);
    Task<IEnumerable<Booking>> GetAllBookingsAsync();
    Task<bool> ConfirmBookingAsync(string bookingId, string paymentId, PaymentMethod paymentMethod);
    Task<bool> CancelBookingAsync(string bookingId, string? reason = null);
}
