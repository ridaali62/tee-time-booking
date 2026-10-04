using Microsoft.AspNetCore.Mvc;
using TeeTimeBooking.Api.Dtos;
using TeeTimeBooking.Api.Services;

namespace TeeTimeBooking.Api.Controllers;

[ApiController]
[Route("bookings")]
public class BookingsController : ControllerBase
{
    private readonly BookingService _service;

    public BookingsController(BookingService service) => _service = service;

    /// <summary>Book a tee time. Returns 400 with the reason when a club rule is broken.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateBookingDto request)
    {
        try
        {
            var b = await _service.BookAsync(request);
            var dto = new BookingDto(b.Id, b.MemberId, b.TeeSlotId, b.TeeSlot!.StartTime, b.Players, b.Guests, b.Status.ToString());
            return Created($"/bookings/{b.Id}", dto);
        }
        catch (BookingRuleException e)
        {
            return BadRequest(new { error = e.Message });
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { error = e.Message });
        }
    }

    /// <summary>Cancel a booking (allowed until 24 hours before the tee time).</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _service.CancelAsync(id);
            return NoContent();
        }
        catch (BookingRuleException e)
        {
            return BadRequest(new { error = e.Message });
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { error = e.Message });
        }
    }
}
