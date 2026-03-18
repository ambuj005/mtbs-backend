using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _service;
    public LocationsController(ILocationService service) { _service = service; }

    [HttpGet("cities")]
    [SwaggerOperation(Summary = "Get all cities")]
    public async Task<IActionResult> GetCities([FromQuery] bool? popular)
    {
        var cities = popular == true ? await _service.GetPopularCitiesAsync() : await _service.GetCitiesAsync();
        return Ok(ApiResponse<IEnumerable<City>>.Ok(cities));
    }

    [HttpGet("cities/{cityId}")]
    [SwaggerOperation(Summary = "Get city by ID")]
    public async Task<IActionResult> GetCityById(string cityId)
    {
        var city = await _service.GetCityByIdAsync(cityId);
        return city == null ? NotFound(ApiResponse<City>.Fail("NOT_FOUND", "City not found")) : Ok(ApiResponse<City>.Ok(city));
    }
}
