using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ShowsController : ControllerBase
{
    private readonly IShowService _service;
    public ShowsController(IShowService service) { _service = service; }

    [HttpGet]
    [SwaggerOperation(Summary = "Get shows")]
    public async Task<IActionResult> GetShows(
        [FromQuery] string? movieId,
        [FromQuery] string? theatreId,
        [FromQuery] DateTime? date)
    {
        return Ok(ApiResponse<IEnumerable<Show>>
            .Ok(await _service.GetShowsAsync(movieId, theatreId, date)));
    }

    [HttpGet("{showId}")]
    [SwaggerOperation(Summary = "Get show by ID")]
    public async Task<IActionResult> GetShowById(string showId, [FromQuery] string theatreId)
    {
        if (string.IsNullOrEmpty(theatreId)) return BadRequest(ApiResponse<Show>.Fail("BAD_REQUEST", "Theatre ID is required"));
        var show = await _service.GetShowByIdAsync(showId, theatreId);
        return show == null ? NotFound(ApiResponse<Show>.Fail("NOT_FOUND", "Show not found")) : Ok(ApiResponse<Show>.Ok(show));
    }
     
}
