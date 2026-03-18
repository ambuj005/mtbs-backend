using System.Text.Json.Serialization;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.DTO;

public class PaymentConfirmationResponse
{
    public bool Success { get; set; }
    public string BookingId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string TicketId { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class PaymentResponse
{
    public string? PaymentId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PaymentStatus Status { get; set; }
    public string? TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum PaymentStatus
{
    Pending,
    Success,
    Failed,
    Refunded
}