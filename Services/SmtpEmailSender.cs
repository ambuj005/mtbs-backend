using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Utils;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Services;

namespace MovieTicketBooking.Api.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendTicketEmailAsync(
        string toEmail,
        string userName,
        string subject,
        string htmlBody,
        string? qrCodeBase64)
    {
        var message = new MimeMessage();
        var fromAddress = _config["Smtp:FromAddress"] ?? string.Empty;
        var fromName = _config["Smtp:FromName"] ?? "Movie Tickets";

        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(new MailboxAddress(userName, toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };

        if (!string.IsNullOrWhiteSpace(qrCodeBase64))
        {
            try
            {
                var base64 = qrCodeBase64;
                if (base64.StartsWith("data:image/png;base64,", StringComparison.OrdinalIgnoreCase))
                {
                    base64 = base64["data:image/png;base64,".Length..];
                }

                base64 = base64.Replace("\r", "").Replace("\n", "").Trim();
                var bytes = Convert.FromBase64String(base64);

                var image = builder.LinkedResources.Add("ticketqr.png", bytes);
                image.ContentId = "ticketqr";
                image.ContentType.MediaType = "image";
                image.ContentType.MediaSubtype = "png";
                image.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to attach QR image for {Email}", toEmail);
            }
        }

        message.Body = builder.ToMessageBody();

        var host = _config["Smtp:Host"];
        var port = _config.GetValue<int?>("Smtp:Port") ?? 587;
        var user = _config["Smtp:User"];
        var password = _config["Smtp:Password"];
        var useSsl = _config.GetValue<bool?>("Smtp:UseSsl") ?? true;

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(host, port,
                useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

            if (!string.IsNullOrWhiteSpace(user))
            {
                await client.AuthenticateAsync(user, password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Ticket email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send ticket email to {Email}", toEmail);
            throw;
        }
    }
}

