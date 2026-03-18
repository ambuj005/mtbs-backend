using MongoDB.Driver;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _db;

    public MongoDbContext(IMongoClient client, IConfiguration configuration)
    {
        var databaseName = configuration["MongoDb:DatabaseName"] ?? "MovieTicketBooking";
        _db = client.GetDatabase(databaseName);
    }

    public IMongoCollection<City> Cities => _db.GetCollection<City>("cities");
    public IMongoCollection<Movie> Movies => _db.GetCollection<Movie>("movies");
    public IMongoCollection<Theatre> Theatres => _db.GetCollection<Theatre>("theatres");
    public IMongoCollection<Show> Shows => _db.GetCollection<Show>("shows");
    public IMongoCollection<SeatLock> SeatLocks => _db.GetCollection<SeatLock>("seatLocks");
    public IMongoCollection<Booking> Bookings => _db.GetCollection<Booking>("bookings");
    public IMongoCollection<Ticket> Tickets => _db.GetCollection<Ticket>("tickets");
    public IMongoCollection<CityMovie> CityMovies => _db.GetCollection<CityMovie>("cityMovies");

    public IMongoCollection<SeatDocument> Seats => _db.GetCollection<SeatDocument>("seats");

    public IMongoCollection<TicketEmailOutboxItem> TicketEmailOutbox =>
        _db.GetCollection<TicketEmailOutboxItem>("ticketEmailOutbox");
}

// The current backend primarily stores seats inside the Show document.
// This model exists only to keep the legacy "seats" collection mapping available.
public class SeatDocument : BaseEntity
{
}
