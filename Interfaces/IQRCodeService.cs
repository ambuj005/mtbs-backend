namespace MovieTicketBooking.Api.Interfaces;

public interface IQRCodeService
{
    string GenerateQRCode(object data);
}
