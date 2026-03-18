using System.Text.Json;
using MovieTicketBooking.Api.Data;
using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Services;

public class OutboxMessagePublisher : IMessagePublisher
{
    private readonly MongoDbContext _db;

    public OutboxMessagePublisher(MongoDbContext db)
    {
        _db = db;
    }

    public async Task PublishTicketEmailAsync(Ticket ticket)
    {
        var message = new TicketEmailMessage
        {
            TicketId = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            BookingId = ticket.BookingId,
            UserEmail = ticket.UserEmail,
            UserName = ticket.UserName,
            MovieTitle = ticket.Movie.Title,
            TheatreName = ticket.Theatre.Name,
            ShowTime = ticket.Show.ShowTime,
            Seats = ticket.Seats.Select(s => $"{s.Row}{s.Number}").ToList(),
            TotalAmount = ticket.TotalAmount,
            QrCodeBase64 = ticket.QrCode,
            CreatedAt = DateTime.UtcNow
        };

        var payload = JsonSerializer.Serialize(message);
        var item = new TicketEmailOutboxItem
        {
            Payload = payload,
            CreatedAt = DateTime.UtcNow
        };

        await _db.TicketEmailOutbox.InsertOneAsync(item);
    }
}
