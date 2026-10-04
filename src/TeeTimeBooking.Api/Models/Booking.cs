namespace TeeTimeBooking.Api.Models;

public enum BookingStatus { Active, Cancelled }

public class Booking
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public Member? Member { get; set; }
    public int TeeSlotId { get; set; }
    public TeeSlot? TeeSlot { get; set; }

    /// <summary>Total players in the group, including the member and guests.</summary>
    public int Players { get; set; }
    public int Guests { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Active;
}
