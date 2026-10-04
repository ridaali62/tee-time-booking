using TeeTimeBooking.Api.Dtos;
using Xunit;
using TeeTimeBooking.Api.Models;
using TeeTimeBooking.Api.Services;

namespace TeeTimeBooking.Tests;

public class BookingServiceTests
{
    private static readonly DateTime Now = new(2026, 10, 10, 9, 0, 0);
    private readonly FakeRepository _repo = new();
    private readonly BookingService _service;

    public BookingServiceTests() => _service = new BookingService(_repo, new FixedTimeProvider(Now));

    private TeeSlot AddSlot(DateTime start)
    {
        var slot = new TeeSlot { Id = _repo.Slots.Count + 1, StartTime = start };
        _repo.Slots.Add(slot);
        return slot;
    }

    private Task<Booking> Book(TeeSlot slot, int players = 2, int guests = 0) =>
        _service.BookAsync(new CreateBookingDto(1, slot.Id, players, guests));

    // ---- booking window ----

    [Fact]
    public async Task Booking_seven_days_ahead_is_allowed()
    {
        var slot = AddSlot(Now.Date.AddDays(7).AddHours(16));
        var booking = await Book(slot);
        Assert.Equal(BookingStatus.Active, booking.Status);
    }

    [Fact]
    public async Task Booking_eight_days_ahead_is_rejected()
    {
        var slot = AddSlot(Now.Date.AddDays(8).AddHours(7));
        await Assert.ThrowsAsync<BookingRuleException>(() => Book(slot));
    }

    [Fact]
    public async Task Booking_a_tee_time_in_the_past_is_rejected()
    {
        var slot = AddSlot(Now.AddMinutes(-10));
        await Assert.ThrowsAsync<BookingRuleException>(() => Book(slot));
    }

    // ---- capacity ----

    [Fact]
    public async Task Four_players_fill_a_slot()
    {
        var slot = AddSlot(Now.AddDays(1));
        await Book(slot, players: 2);
        await Book(slot, players: 2);
        Assert.Equal(4, await _repo.BookedPlayersAsync(slot.Id));
    }

    [Fact]
    public async Task Booking_more_than_four_players_in_a_slot_is_rejected()
    {
        var slot = AddSlot(Now.AddDays(1));
        await Book(slot, players: 3);
        var error = await Assert.ThrowsAsync<BookingRuleException>(() => Book(slot, players: 2));
        Assert.Contains("1 place(s) left", error.Message);
    }

    [Fact]
    public async Task Cancelled_bookings_free_their_places()
    {
        var slot = AddSlot(Now.AddDays(2));
        var first = await Book(slot, players: 4);
        await _service.CancelAsync(first.Id);
        var second = await Book(slot, players: 4);
        Assert.Equal(BookingStatus.Active, second.Status);
    }

    // ---- guests ----

    [Fact]
    public async Task Two_guests_are_allowed()
    {
        var slot = AddSlot(Now.AddDays(1));
        var booking = await Book(slot, players: 3, guests: 2);
        Assert.Equal(2, booking.Guests);
    }

    [Fact]
    public async Task Three_guests_are_rejected()
    {
        var slot = AddSlot(Now.AddDays(1));
        await Assert.ThrowsAsync<BookingRuleException>(() => Book(slot, players: 4, guests: 3));
    }

    [Fact]
    public async Task Guests_must_be_fewer_than_players()
    {
        var slot = AddSlot(Now.AddDays(1));
        await Assert.ThrowsAsync<BookingRuleException>(() => Book(slot, players: 2, guests: 2));
    }

    // ---- cancellation cut-off ----

    [Fact]
    public async Task Cancelling_more_than_24_hours_before_is_allowed()
    {
        var slot = AddSlot(Now.AddHours(25));
        var booking = await Book(slot);
        await _service.CancelAsync(booking.Id);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact]
    public async Task Cancelling_less_than_24_hours_before_is_rejected()
    {
        var slot = AddSlot(Now.AddHours(23));
        var booking = await Book(slot);
        await Assert.ThrowsAsync<BookingRuleException>(() => _service.CancelAsync(booking.Id));
    }

    [Fact]
    public async Task Cancelling_twice_is_rejected()
    {
        var slot = AddSlot(Now.AddDays(3));
        var booking = await Book(slot);
        await _service.CancelAsync(booking.Id);
        await Assert.ThrowsAsync<BookingRuleException>(() => _service.CancelAsync(booking.Id));
    }

    // ---- weather delay and lookups ----

    [Fact]
    public async Task Weather_delay_flag_can_be_set_and_shows_in_slot_list()
    {
        var slot = AddSlot(Now.AddDays(1));
        await _service.SetWeatherDelayAsync(slot.Id, true);
        var slots = await _service.GetSlotsAsync(DateOnly.FromDateTime(slot.StartTime));
        Assert.True(Assert.Single(slots).WeatherDelay);
    }

    [Fact]
    public async Task Slot_list_shows_booked_players()
    {
        var slot = AddSlot(Now.AddDays(1));
        await Book(slot, players: 3, guests: 1);
        var slots = await _service.GetSlotsAsync(DateOnly.FromDateTime(slot.StartTime));
        Assert.Equal(3, Assert.Single(slots).BookedPlayers);
    }

    [Fact]
    public async Task Unknown_slot_or_member_is_not_found()
    {
        var slot = AddSlot(Now.AddDays(1));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.BookAsync(new CreateBookingDto(1, 999, 1, 0)));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.BookAsync(new CreateBookingDto(42, slot.Id, 1, 0)));
    }
}
