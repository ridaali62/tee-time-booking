using TeeTimeBooking.Api.Dtos;
using TeeTimeBooking.Api.Models;
using TeeTimeBooking.Api.Repositories;

namespace TeeTimeBooking.Api.Services;

/// <summary>All club booking rules live here, so they can be unit-tested without a database or HTTP.</summary>
public class BookingService
{
    public const int MaxDaysAhead = 7;
    public const int MaxGuestsPerBooking = 2;
    public static readonly TimeSpan CancellationCutoff = TimeSpan.FromHours(24);

    private readonly ITeeTimeRepository _repo;
    private readonly TimeProvider _time;

    public BookingService(ITeeTimeRepository repo, TimeProvider time)
    {
        _repo = repo;
        _time = time;
    }

    private DateTime Now => _time.GetLocalNow().DateTime;

    public async Task<List<SlotDto>> GetSlotsAsync(DateOnly date)
    {
        var slots = await _repo.GetSlotsAsync(date);
        return slots.Select(s => new SlotDto(
            s.Id, s.StartTime, s.Course, s.Capacity,
            s.Bookings.Where(b => b.Status == BookingStatus.Active).Sum(b => b.Players),
            s.WeatherDelay)).ToList();
    }

    public async Task<Booking> BookAsync(CreateBookingDto request)
    {
        var slot = await _repo.GetSlotAsync(request.TeeSlotId)
            ?? throw new KeyNotFoundException($"Tee slot {request.TeeSlotId} not found.");

        if (!await _repo.MemberExistsAsync(request.MemberId))
            throw new KeyNotFoundException($"Member {request.MemberId} not found.");

        if (slot.StartTime <= Now)
            throw new BookingRuleException("This tee time has already started.");

        if (slot.StartTime.Date > Now.Date.AddDays(MaxDaysAhead))
            throw new BookingRuleException($"Tee times can be booked at most {MaxDaysAhead} days ahead.");

        if (request.Players < 1)
            throw new BookingRuleException("A booking needs at least 1 player.");

        if (request.Guests > MaxGuestsPerBooking)
            throw new BookingRuleException($"A booking can include at most {MaxGuestsPerBooking} guests.");

        if (request.Guests >= request.Players)
            throw new BookingRuleException("The member must be one of the players, so guests must be fewer than players.");

        var booked = await _repo.BookedPlayersAsync(slot.Id);
        if (booked + request.Players > slot.Capacity)
            throw new BookingRuleException($"Only {slot.Capacity - booked} place(s) left in this tee time.");

        var booking = new Booking
        {
            MemberId = request.MemberId,
            TeeSlotId = slot.Id,
            TeeSlot = slot,
            Players = request.Players,
            Guests = request.Guests,
        };
        await _repo.AddBookingAsync(booking);
        await _repo.SaveChangesAsync();
        return booking;
    }

    public async Task CancelAsync(int bookingId)
    {
        var booking = await _repo.GetBookingAsync(bookingId)
            ?? throw new KeyNotFoundException($"Booking {bookingId} not found.");

        if (booking.Status == BookingStatus.Cancelled)
            throw new BookingRuleException("This booking is already cancelled.");

        if (booking.TeeSlot!.StartTime - Now < CancellationCutoff)
            throw new BookingRuleException("Bookings can only be cancelled up to 24 hours before the tee time.");

        booking.Status = BookingStatus.Cancelled;
        await _repo.SaveChangesAsync();
    }

    public async Task<TeeSlot> SetWeatherDelayAsync(int slotId, bool weatherDelay)
    {
        var slot = await _repo.GetSlotAsync(slotId)
            ?? throw new KeyNotFoundException($"Tee slot {slotId} not found.");
        slot.WeatherDelay = weatherDelay;
        await _repo.SaveChangesAsync();
        return slot;
    }
}
