using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Interfaces;

public interface IMessagePublisher
{
    Task PublishTicketEmailAsync(Ticket ticket);
}
