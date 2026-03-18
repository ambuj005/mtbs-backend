using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MovieTicketBooking.Api.Models;

public class TicketEmailOutboxItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("payload")]
    public string Payload { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("processedAt")]
    public DateTime? ProcessedAt { get; set; }

    [BsonElement("error")]
    public string? Error { get; set; }
}
