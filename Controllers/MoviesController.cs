using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _service;

    public MoviesController(IMovieService service)
    {
        _service = service;
    }

    [HttpGet("by-city/{cityId}")]
    [SwaggerOperation(Summary = "Get all movies available in a city")]
    public async Task<IActionResult> GetMoviesByCity(string cityId)
    {
        return Ok(ApiResponse<IEnumerable<MovieListItemDTO>>
            .Ok(await _service.GetMoviesByCityAsync(cityId)));
    }

    [HttpGet("by-city/{cityId}/paginated")]
    [SwaggerOperation(Summary = "Get movies available in a city with pagination and filters")]
    public async Task<IActionResult> GetMoviesByCityPaginated(
    string cityId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 4,
    [FromQuery] string? search = null,
    [FromQuery] string? genre = null,
    [FromQuery] string? language = null)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;
        
        var result = await _service.GetMoviesByCityPaginatedAsync(
            cityId,
            page,
            pageSize,
            search,
            genre,
            language
        );

        return Ok(ApiResponse<PaginatedMoviesResponse>.Ok(result));
    }

    [HttpGet("by-city/{cityId}/search")]
    [SwaggerOperation(Summary = "Search movies available in a city")]
    public async Task<IActionResult> SearchMoviesByCity(
        string cityId,
        [FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
        {
            return BadRequest(ApiResponse<IEnumerable<MovieListItemDTO>>
                .Fail("BAD_REQUEST", "Search query must be at least 2 characters"));
        }

        return Ok(ApiResponse<IEnumerable<MovieListItemDTO>>
            .Ok(await _service.SearchMoviesByCityAsync(cityId, q)));
    }

    [HttpGet("{movieId}")]
    [SwaggerOperation(Summary = "Get movie by ID")]
    public async Task<IActionResult> GetMovieById(string movieId)
    {
        var movie = await _service.GetMovieByIdAsync(movieId);

        return movie == null
            ? NotFound(ApiResponse<Movie>.Fail("NOT_FOUND", "Movie not found"))
            : Ok(ApiResponse<Movie>.Ok(movie));
    }
}
