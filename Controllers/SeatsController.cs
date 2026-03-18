using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Interfaces;

namespace MovieTicketBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SeatsController : ControllerBase
{
    private readonly ISeatService _service;
    public SeatsController(ISeatService service) { _service = service; }

    [HttpPost("lock")]
    [SwaggerOperation(Summary = "Lock seats")]
    public async Task<IActionResult> LockSeats([FromBody] LockSeatsRequest req)
    {
        var result = await _service.LockSeatsAsync(
            req.ShowId,
            req.SeatIds,
            req.SessionId
        );

        return result.Success
            ? Ok(ApiResponse<LockSeatsResponse>.Ok(result))
            : Conflict(ApiResponse<LockSeatsResponse>.Fail(
                "SEATS_UNAVAILABLE",
                result.Message ?? "Some seats are not available"));
    }

    [HttpPost("unlock")]
    [SwaggerOperation(Summary = "Unlock seats")]
    public async Task<IActionResult> UnlockSeats([FromBody] UnlockSeatsRequest req)
    {
        return await _service.UnlockSeatsAsync(req.LockId, req.SessionId)
            ? Ok(ApiResponse<object>.Ok(new { success = true }))
            : NotFound(ApiResponse<object>.Fail(
                "NOT_FOUND",
                "Lock not found or already released"));
    }

    [HttpGet("show/{showId}")]
    [SwaggerOperation(Summary = "Get seat layout for a show")]
    public async Task<IActionResult> GetSeatLayout(string showId)
    {
        var layout = await _service.GetSeatLayoutAsync(showId);

        if (layout == null)
            return NotFound(ApiResponse<object>.Fail("NOT_FOUND", "No seats found for show"));

        return Ok(ApiResponse<SeatLayoutResponse>.Ok(layout));
    }


    [HttpPost("extend-lock")]
    [SwaggerOperation(Summary = "Extend seat lock")]
    public async Task<IActionResult> ExtendLock([FromBody] ExtendLockRequest req)
    {
        return await _service.ExtendLockAsync(
            req.LockId,
            req.SessionId,
            req.AdditionalMinutes)
            ? Ok(ApiResponse<object>.Ok(new { success = true }))
            : NotFound(ApiResponse<object>.Fail(
                "NOT_FOUND",
                "Lock not found or expired"));
    }
}
