using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _service;
    public BookingsController(IBookingService service)
    {
        _service = service;
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create booking")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest req)
    {
        try
        {
            return Ok(
                ApiResponse<BookingResponse>.Ok(
                    await _service.CreateBookingAsync(req)
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(
                ApiResponse<BookingResponse>.Fail("BOOKING_FAILED", ex.Message)
            );
        }
    }

    [HttpGet("{bookingId}")]
    [SwaggerOperation(Summary = "Get booking by ID")]
    public async Task<IActionResult> GetBookingById(string bookingId)
    {
        var booking = await _service.GetBookingByIdAsync(bookingId);

        return booking == null
            ? NotFound(ApiResponse<Booking>.Fail("NOT_FOUND", "Booking not found"))
            : Ok(ApiResponse<Booking>.Ok(booking));
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Get all bookings")]
    public async Task<IActionResult> GetAllBookings()
    {
        return Ok(
            ApiResponse<IEnumerable<Booking>>.Ok(
                await _service.GetAllBookingsAsync()
            )
        );
    }

    [HttpPost("{bookingId}/cancel")]
    [SwaggerOperation(Summary = "Cancel booking")]
    public async Task<IActionResult> CancelBooking(
        string bookingId,
        [FromBody] CancelBookingRequest? req
    )
    {
        return await _service.CancelBookingAsync(bookingId, req?.Reason)
            ? Ok(ApiResponse<object>.Ok(new { success = true }))
            : NotFound(
                ApiResponse<object>.Fail(
                    "NOT_FOUND",
                    "Booking not found or cannot be cancelled"
                )
            );
    }
}
