# Tee-Time Booking

A golf club tee-time booking system: an **ASP.NET Core 8 Web API** with **Entity Framework Core** on **SQL Server**, an **Angular** booking screen and **xUnit** tests for every booking rule.

> The club, its members and its rules are fictional sample data.

## Club rules enforced by the API

| Rule | Where it's checked | Test |
|---|---|---|
| Book at most **7 days ahead**; no tee times in the past | `BookingService.BookAsync` | `Booking_seven_days_ahead_is_allowed`, `Booking_eight_days_ahead_is_rejected`, `Booking_a_tee_time_in_the_past_is_rejected` |
| **Max 4 players** per tee slot (cancelled bookings free their places) | `BookingService.BookAsync` | `Four_players_fill_a_slot`, `Booking_more_than_four_players_in_a_slot_is_rejected`, `Cancelled_bookings_free_their_places` |
| **Max 2 guests** per booking; the member must be one of the players | `BookingService.BookAsync` | `Two_guests_are_allowed`, `Three_guests_are_rejected`, `Guests_must_be_fewer_than_players` |
| Cancel only until **24 hours before** the tee time | `BookingService.CancelAsync` | `Cancelling_more_than_24_hours_before_is_allowed`, `Cancelling_less_than_24_hours_before_is_rejected`, `Cancelling_twice_is_rejected` |
| **Weather-delay** flag on a slot, shown to members as a banner | `BookingService.SetWeatherDelayAsync` | `Weather_delay_flag_can_be_set_and_shows_in_slot_list` |

## Design

```
Angular client ──HTTP/JSON──► Controllers ──► BookingService (rules) ──► ITeeTimeRepository ──► EF Core ──► SQL Server
```

- **Controllers** (`SlotsController`, `BookingsController`) only translate HTTP: they call the service and map rule violations to `400 Bad Request` and missing records to `404 Not Found`.
- **`BookingService`** holds every club rule in one place. It depends on `ITeeTimeRepository` and .NET's `TimeProvider`, so tests swap in an in-memory repository and a frozen clock. No database or HTTP is needed to test the rules.
- **`TeeTimeRepository`** is the only class that talks to EF Core.
- **DTOs** (`Dtos/Dtos.cs`) keep the API contract separate from the database entities; data annotations validate the request body.
- **Cancelling** sets `Status = Cancelled` instead of deleting the row, so the booking history is kept.

## Endpoints

| Method | Path | What it does |
|---|---|---|
| GET | `/slots?date=2026-10-10` | Tee slots on a date with booked players and the weather-delay flag |
| POST | `/bookings` | Book: `{ "memberId": 1, "teeSlotId": 12, "players": 3, "guests": 2 }` |
| DELETE | `/bookings/{id}` | Cancel a booking |
| PATCH | `/slots/{id}/weather-delay` | Staff: `{ "weatherDelay": true }` |

Swagger UI: http://localhost:5080/swagger

## Run it

**Requirements:** .NET 8 SDK, SQL Server (Express LocalDB, which comes with Visual Studio, or Docker), Node.js 20+.

1. **API** (creates the database and demo data on first start):
   ```
   cd src/TeeTimeBooking.Api
   dotnet run
   ```
   With SQL Server in Docker instead of LocalDB, change `ConnectionStrings:Club` in `appsettings.json` to
   `Server=localhost,1433;Database=TeeTimeBooking;User Id=sa;Password=<your password>;TrustServerCertificate=True`.
2. **Tests:** `dotnet test` (from the repository root)
3. **Angular client:**
   ```
   cd client
   npm install
   npm start
   ```
   Open http://localhost:4200.

## CI

`.github/workflows/ci.yml` builds the API, runs the xUnit tests and builds the Angular client on every push and pull request.

## Next steps

- Lottery for weekend morning tee times
- EF Core migrations instead of `EnsureCreated`
- Authentication so members can only cancel their own bookings
- Concurrency check so two members can't take the last place at the same moment
