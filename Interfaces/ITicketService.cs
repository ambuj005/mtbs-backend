using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Interfaces;

public interface ITicketService
{
    Task<Ticket> GenerateTicketAsync(Booking booking);
    Task<Ticket?> GetTicketByIdAsync(string ticketId);
    Task<Ticket?> GetTicketByBookingAsync(string bookingId);
    Task<IEnumerable<Ticket>> GetAllTicketsAsync();
    Task<VerifyQRResponse> VerifyQRCodeAsync(VerifyQRRequest request);
}
