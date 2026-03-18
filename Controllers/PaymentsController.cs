using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;
using Razorpay.Api;
using Swashbuckle.AspNetCore.Annotations;

namespace MovieTicketBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ISeatLockRepository _lockRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentService paymentService,
        ISeatLockRepository lockRepo,
        IBookingRepository bookingRepo,
        IConfiguration configuration,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _lockRepo = lockRepo;
        _bookingRepo = bookingRepo;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("create-order")]
    [SwaggerOperation(Summary = "Create Razorpay order for locked seats")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        { 
            var keyId = _configuration["Razorpay:KeyId"];
            var keySecret = _configuration["Razorpay:KeySecret"];

            if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
            {
                return StatusCode(500, ApiResponse<object>.Fail(
                    "CONFIG_ERROR",
                    "Razorpay keys missing"
                ));
            }

            var locks = await _lockRepo.QueryAsync(q =>
                q.Where(l =>
                    l.Id == request.LockId &&
                    l.Status == SeatLockStatus.Active &&
                    l.ExpiresAt > DateTime.UtcNow
                )
            );

            var seatLock = locks.FirstOrDefault();
            if (seatLock == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    "LOCK_NOT_FOUND",
                    "Seat lock not found or expired"
                ));
            }
             
            var amountInPaise = Convert.ToInt32(Math.Round(request.Amount * 100));

            var client = new RazorpayClient(keyId, keySecret);

            var options = new Dictionary<string, object>
        {
            { "amount", amountInPaise },
            { "currency", "INR" },
            { "receipt", $"rcpt_{Guid.NewGuid():N}" },
            { "payment_capture", 1 }
        };

            var order = client.Order.Create(options);

            var razorpayOrderId = order["id"]?.ToString();
            if (string.IsNullOrWhiteSpace(razorpayOrderId))
            {
                throw new InvalidOperationException("Failed to create Razorpay order");
            }
             
            seatLock.RazorpayOrderId = razorpayOrderId;
            await _lockRepo.UpdateAsync(seatLock);
             
            return Ok(ApiResponse<object>.Ok(new
            {
                orderId = razorpayOrderId,
                amount = amountInPaise,
                currency = "INR"
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Razorpay order creation failed");
            return StatusCode(500, ApiResponse<object>.Fail(
                "ORDER_FAILED",
                ex.Message
            ));
        }
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentRequest req)
    {
        try
        {
            var isMock = _configuration.GetValue<bool>("Razorpay:Mock");

            PaymentConfirmationResponse result;

            if (isMock)
            {
                result = await _paymentService.ProcessMockPaymentAsync(
                    req.BookingDetails,
                    req.RazorpayPaymentId,
                    req.RazorpayOrderId
                );
            }
            else
            {
                result = await _paymentService.ProcessPaymentWithBookingAsync(
                    req.BookingDetails,
                    req.RazorpayPaymentId,
                    req.RazorpayOrderId,
                    req.RazorpaySignature
                );
            }

            return Ok(ApiResponse<PaymentConfirmationResponse>.Ok(result));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail("PAYMENT_FAILED", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment verification failed");
            return StatusCode(500,
                ApiResponse<object>.Fail("VERIFICATION_FAILED", "Payment verification failed"));
        }
    }
}

public class CreateOrderRequest
{
    public string LockId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class VerifyPaymentRequest
{
    public CreateBookingRequest BookingDetails { get; set; } = new();
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public string RazorpayOrderId { get; set; } = string.Empty;
    public string RazorpaySignature { get; set; } = string.Empty;
}