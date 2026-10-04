using Microsoft.EntityFrameworkCore;
using TeeTimeBooking.Api.Data;
using TeeTimeBooking.Api.Models;

namespace TeeTimeBooking.Api.Repositories;

public class TeeTimeRepository : ITeeTimeRepository
{
    private readonly ClubDbContext _db;

    public TeeTimeRepository(ClubDbContext db) => _db = db;

    public Task<List<TeeSlot>> GetSlotsAsync(DateOnly date)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        return _db.TeeSlots
            .Include(s => s.Bookings)
            .Where(s => s.StartTime >= start && s.StartTime < start.AddDays(1))
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public Task<TeeSlot?> GetSlotAsync(int id) => _db.TeeSlots.FirstOrDefaultAsync(s => s.Id == id);

    public Task<int> BookedPlayersAsync(int slotId) =>
        _db.Bookings.Where(b => b.TeeSlotId == slotId && b.Status == BookingStatus.Active).SumAsync(b => b.Players);

    public Task<bool> MemberExistsAsync(int memberId) => _db.Members.AnyAsync(m => m.Id == memberId);

    public Task<Booking?> GetBookingAsync(int id) =>
        _db.Bookings.Include(b => b.TeeSlot).FirstOrDefaultAsync(b => b.Id == id);

    public async Task AddBookingAsync(Booking booking) => await _db.Bookings.AddAsync(booking);

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
