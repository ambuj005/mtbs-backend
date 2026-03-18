using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _service;
    public TicketsController(ITicketService service) { _service = service; }

    [HttpGet]
    [SwaggerOperation(Summary = "Get all tickets")]
    public async Task<IActionResult> GetTickets()
    {
        return Ok(ApiResponse<IEnumerable<Ticket>>.Ok(await _service.GetAllTicketsAsync()));
    }

    [HttpGet("{ticketId}")]
    [SwaggerOperation(Summary = "Get ticket by ID")]
    public async Task<IActionResult> GetTicketById(string ticketId)
    {
        var ticket = await _service.GetTicketByIdAsync(ticketId);
        return ticket == null
            ? NotFound(ApiResponse<Ticket>.Fail("NOT_FOUND", "Ticket not found"))
            : Ok(ApiResponse<Ticket>.Ok(ticket));
    }

    [HttpGet("booking/{bookingId}")]
    [SwaggerOperation(Summary = "Get ticket by booking ID")]
    public async Task<IActionResult> GetTicketByBooking(string bookingId)
    {
        var ticket = await _service.GetTicketByBookingAsync(bookingId);
        return ticket == null
            ? NotFound(ApiResponse<Ticket>.Fail("NOT_FOUND", "Ticket not found"))
            : Ok(ApiResponse<Ticket>.Ok(ticket));
    }

    [HttpPost("verify")]
    [SwaggerOperation(
        Summary = "Verify and validate ticket")]
    public async Task<IActionResult> VerifyQRCode([FromBody] VerifyQRRequest req)
    {
        if (string.IsNullOrEmpty(req.QrData) && string.IsNullOrEmpty(req.TicketId))
        {
            return BadRequest(ApiResponse<VerifyQRResponse>.Fail(
                "BAD_REQUEST",
                "Either QR data or Ticket ID is required"));
        }

        if (string.IsNullOrEmpty(req.VerifiedBy))
        {
            return BadRequest(ApiResponse<VerifyQRResponse>.Fail(
                "BAD_REQUEST",
                "VerifiedBy field is required"));
        }

        var response = await _service.VerifyQRCodeAsync(req);

        return Ok(ApiResponse<VerifyQRResponse>.Ok(response));
    }
}
