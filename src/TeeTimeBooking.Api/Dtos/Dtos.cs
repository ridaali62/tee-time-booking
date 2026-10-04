using System.ComponentModel.DataAnnotations;

namespace TeeTimeBooking.Api.Dtos;

public record SlotDto(int Id, DateTime StartTime, string Course, int Capacity, int BookedPlayers, bool WeatherDelay);

public record CreateBookingDto(
    [Range(1, int.MaxValue)] int MemberId,
    [Range(1, int.MaxValue)] int TeeSlotId,
    [Range(1, 4)] int Players,
    [Range(0, 3)] int Guests);

public record BookingDto(int Id, int MemberId, int TeeSlotId, DateTime StartTime, int Players, int Guests, string Status);

public record WeatherDelayDto(bool WeatherDelay);
