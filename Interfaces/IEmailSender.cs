namespace MovieTicketBooking.Api.Interfaces;

public interface IEmailSender
{
    Task SendTicketEmailAsync(string toEmail, string userName, string subject, string htmlBody, string? qrCodeBase64);
}
