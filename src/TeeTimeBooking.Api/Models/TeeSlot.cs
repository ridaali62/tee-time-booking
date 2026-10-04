namespace TeeTimeBooking.Api.Models;

public class TeeSlot
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public string Course { get; set; } = "Main";
    public int Capacity { get; set; } = 4;
    public bool WeatherDelay { get; set; }
    public List<Booking> Bookings { get; set; } = new();
}
