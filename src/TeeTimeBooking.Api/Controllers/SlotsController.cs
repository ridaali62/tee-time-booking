using Microsoft.AspNetCore.Mvc;
using TeeTimeBooking.Api.Dtos;
using TeeTimeBooking.Api.Services;

namespace TeeTimeBooking.Api.Controllers;

[ApiController]
[Route("slots")]
public class SlotsController : ControllerBase
{
    private readonly BookingService _service;

    public SlotsController(BookingService service) => _service = service;

    /// <summary>All tee slots on a date, with how many players are already booked.</summary>
    [HttpGet]
    public Task<List<SlotDto>> Get([FromQuery] DateOnly date) => _service.GetSlotsAsync(date);

    /// <summary>Turn the weather-delay notice for a slot on or off.</summary>
    [HttpPatch("{id:int}/weather-delay")]
    public async Task<IActionResult> SetWeatherDelay(int id, WeatherDelayDto body)
    {
        try
        {
            var slot = await _service.SetWeatherDelayAsync(id, body.WeatherDelay);
            return Ok(new { slot.Id, slot.WeatherDelay });
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { error = e.Message });
        }
    }
}
