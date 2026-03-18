using MovieTicketBooking.Api.Interfaces;
using QRCoder;
using System.Text.Json;

namespace MovieTicketBooking.Api.Services;

public class QRCodeService : IQRCodeService
{
    public string GenerateQRCode(object data)
    {
        var payload = data is string s
            ? s
            : JsonSerializer.Serialize(data);

        using var generator = new QRCodeGenerator();
        using var qrData = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.H);
        using var qrCode = new Base64QRCode(qrData);

        var base64 = qrCode.GetGraphic(20);
         
        return $"data:image/png;base64,{base64}";
    }
}
