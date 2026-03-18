using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TheatresController : ControllerBase
{
    private readonly ITheatreService _service;
    public TheatresController(ITheatreService service) { _service = service; }

    [HttpGet]
    [SwaggerOperation(Summary = "Get theatres in a city")]
    public async Task<IActionResult> GetTheatres(
        [FromQuery] string cityId,
        [FromQuery] string? areaId)
    {
        if (string.IsNullOrEmpty(cityId))
            return BadRequest(ApiResponse<IEnumerable<Theatre>>
                .Fail("BAD_REQUEST", "City ID is required"));

        return Ok(ApiResponse<IEnumerable<Theatre>>
            .Ok(await _service.GetTheatresAsync(cityId, areaId)));
    }

    [HttpGet("{theatreId}")]
    [SwaggerOperation(Summary = "Get theatre by ID")]
    public async Task<IActionResult> GetTheatreById(string theatreId, [FromQuery] string cityId)
    {
        if (string.IsNullOrEmpty(cityId)) return BadRequest(ApiResponse<Theatre>.Fail("BAD_REQUEST", "City ID is required"));
        var theatre = await _service.GetTheatreByIdAsync(theatreId, cityId);
        return theatre == null ? NotFound(ApiResponse<Theatre>.Fail("NOT_FOUND", "Theatre not found")) : Ok(ApiResponse<Theatre>.Ok(theatre));
    }

    [HttpGet("movie/{movieId}")]
    public async Task<IActionResult> GetTheatresWithShows(
        string movieId,
        [FromQuery] string cityId,
        [FromQuery] DateTime? date) 
    {
        if (string.IsNullOrEmpty(cityId))
            return BadRequest(ApiResponse<IEnumerable<ShowWithTheatreDTO>>
                .Fail("BAD_REQUEST", "City ID is required"));
        return Ok(ApiResponse<IEnumerable<ShowWithTheatreDTO>>
            .Ok(await _service.GetTheatresWithShowsAsync(movieId, cityId, date)));
    }
}
