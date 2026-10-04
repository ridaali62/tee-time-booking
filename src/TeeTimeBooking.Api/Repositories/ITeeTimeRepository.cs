using TeeTimeBooking.Api.Models;

namespace TeeTimeBooking.Api.Repositories;

public interface ITeeTimeRepository
{
    Task<List<TeeSlot>> GetSlotsAsync(DateOnly date);
    Task<TeeSlot?> GetSlotAsync(int id);
    Task<int> BookedPlayersAsync(int slotId);
    Task<bool> MemberExistsAsync(int memberId);
    Task<Booking?> GetBookingAsync(int id);
    Task AddBookingAsync(Booking booking);
    Task SaveChangesAsync();
}
