namespace TeeTimeBooking.Api.Services;

/// <summary>Thrown when a request breaks a club booking rule. The API returns it as 400 Bad Request.</summary>
public class BookingRuleException : Exception
{
    public BookingRuleException(string message) : base(message) { }
}
