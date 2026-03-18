using MovieTicketBooking.Api.DTO;

public interface IPaymentService
{  
    Task<PaymentResponse> GetPaymentStatusAsync(string paymentId); 
    Task<PaymentConfirmationResponse> ProcessPaymentWithBookingAsync(
        CreateBookingRequest bookingDetails,
        string razorpayPaymentId,
        string razorpayOrderId,
        string razorpaySignature
    ); 
    Task<PaymentConfirmationResponse> ProcessMockPaymentAsync(
        CreateBookingRequest bookingDetails,
        string razorpayPaymentId,
        string razorpayOrderId
    );
}
