using System.Globalization;
using MovieTicketBooking.Api.DTO;

namespace MovieTicketBooking.Api.Services;

public static class TicketEmailTemplateBuilder
{
    private const string HtmlTemplate = @"<!DOCTYPE html>
<html>
<head><meta charset=""UTF-8"" /><meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" /><title>Your Movie Ticket</title></head>
<body style=""margin:0;padding:0;background:#0b0b0b;font-family:Arial,sans-serif;color:#ffffff;"">
<table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#0b0b0b;""><tr><td align=""center"" style=""padding:20px;"">
<table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background:#121212;border-radius:12px;padding:24px;max-width:100%;"">
<tr><td align=""center"" style=""padding-bottom:20px;""><h1 style=""color:#e50914;margin:0 0 8px 0;font-size:28px;"">🎟️ Your Movie Ticket</h1><p style=""color:#bbbbbb;margin:0;font-size:16px;"">Hi {{UserName}}, your booking is confirmed!</p></td></tr>
<tr><td style=""padding:20px;background:#1a1a1a;border-radius:8px;""><table width=""100%"" cellpadding=""8"" cellspacing=""0"">
<tr><td style=""color:#888;font-size:14px;padding:6px 0;"">🎬 Movie:</td><td style=""color:#fff;font-size:14px;padding:6px 0;font-weight:bold;"">{{MovieTitle}}</td></tr>
<tr><td style=""color:#888;font-size:14px;padding:6px 0;"">🏢 Theatre:</td><td style=""color:#fff;font-size:14px;padding:6px 0;"">{{TheatreName}}</td></tr>
<tr><td style=""color:#888;font-size:14px;padding:6px 0;"">🕒 Show Time:</td><td style=""color:#fff;font-size:14px;padding:6px 0;"">{{ShowTime}}</td></tr>
<tr><td style=""color:#888;font-size:14px;padding:6px 0;"">💺 Seats:</td><td style=""color:#e50914;font-size:14px;padding:6px 0;font-weight:bold;"">{{Seats}}</td></tr>
<tr><td style=""color:#888;font-size:14px;padding:6px 0;"">💳 Amount Paid:</td><td style=""color:#4caf50;font-size:14px;padding:6px 0;font-weight:bold;"">₹{{TotalAmount}}</td></tr>
<tr><td style=""color:#888;font-size:14px;padding:6px 0;"">🧾 Ticket No:</td><td style=""color:#fff;font-size:13px;padding:6px 0;font-family:monospace;"">{{TicketNumber}}</td></tr>
</table></td></tr>
<tr><td align=""center"" style=""padding-top:30px;""><p style=""margin:0 0 16px 0;color:#bbbbbb;font-size:16px;font-weight:bold;"">Scan this QR code at the theatre:</p><img src=""cid:ticketqr"" alt=""Ticket QR Code"" width=""220"" height=""220"" style=""display:block;margin:0 auto;background:#ffffff;padding:10px;border-radius:8px;border:2px solid #e50914;"" /></td></tr>
<tr><td align=""center"" style=""padding-top:30px;color:#888;font-size:13px;line-height:1.6;"">Please arrive 15 minutes early.<br />Enjoy your movie 🍿<br /><br /><span style=""font-size:11px;color:#666;"">This is an automated email. Please do not reply.</span></td></tr>
</table></td></tr></table></body></html>";

    public static string Build(TicketEmailMessage ticket)
    {
        var html = HtmlTemplate
            .Replace("{{UserName}}", EscapeHtml(ticket.UserName))
            .Replace("{{MovieTitle}}", EscapeHtml(ticket.MovieTitle))
            .Replace("{{TheatreName}}", EscapeHtml(ticket.TheatreName))
            .Replace("{{TicketNumber}}", EscapeHtml(ticket.TicketNumber))
            .Replace("{{Seats}}", EscapeHtml(string.Join(", ", ticket.Seats)))
            .Replace("{{TotalAmount}}", ticket.TotalAmount.ToString("0.00", CultureInfo.InvariantCulture))
            .Replace("{{ShowTime}}", ticket.ShowTime.ToString("dd MMM yyyy, hh:mm tt"));
        return html;
    }

    private static string EscapeHtml(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}
