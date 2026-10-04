using TeeTimeBooking.Api.Models;
using TeeTimeBooking.Api.Repositories;

namespace TeeTimeBooking.Tests;

/// <summary>In-memory repository so the booking rules can be tested without SQL Server.</summary>
public class FakeRepository : ITeeTimeRepository
{
    public List<Member> Members { get; } = new() { new Member { Id = 1, Name = "Test Member" } };
    public List<TeeSlot> Slots { get; } = new();
    public List<Booking> Bookings { get; } = new();

    public Task<List<TeeSlot>> GetSlotsAsync(DateOnly date) =>
        Task.FromResult(Slots.Where(s => DateOnly.FromDateTime(s.StartTime) == date).ToList());

    public Task<TeeSlot?> GetSlotAsync(int id) => Task.FromResult(Slots.FirstOrDefault(s => s.Id == id));

    public Task<int> BookedPlayersAsync(int slotId) =>
        Task.FromResult(Bookings.Where(b => b.TeeSlotId == slotId && b.Status == BookingStatus.Active).Sum(b => b.Players));

    public Task<bool> MemberExistsAsync(int memberId) => Task.FromResult(Members.Any(m => m.Id == memberId));

    public Task<Booking?> GetBookingAsync(int id) => Task.FromResult(Bookings.FirstOrDefault(b => b.Id == id));

    public Task AddBookingAsync(Booking booking)
    {
        booking.Id = Bookings.Count + 1;
        Bookings.Add(booking);
        booking.TeeSlot?.Bookings.Add(booking);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => Task.CompletedTask;
}

/// <summary>A clock frozen at a chosen time, so "7 days ahead" and "24 hours before" are testable.</summary>
public class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _now;

    public FixedTimeProvider(DateTime localNow) => _now = new DateTimeOffset(localNow, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => _now;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
