using System.Security.Cryptography;
using System.Text;

namespace MovieTicketBooking.Api.Utils;

public static class RazorpaySignatureVerifier
{
    public static bool Verify(
        string orderId,
        string paymentId,
        string razorpaySignature,
        string keySecret)
    {
        var payload = $"{orderId}|{paymentId}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keySecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));

        var generatedSignature = BitConverter
            .ToString(hash)
            .Replace("-", "")
            .ToLowerInvariant();

        return generatedSignature == razorpaySignature;
    }
}
