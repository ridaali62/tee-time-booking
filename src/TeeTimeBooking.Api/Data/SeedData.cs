using TeeTimeBooking.Api.Models;

namespace TeeTimeBooking.Api.Data;

public static class SeedData
{
    /// <summary>Creates demo members and tee slots every 10 minutes (6:30 am - 5:00 pm) for the next 8 days.</summary>
    public static void Seed(ClubDbContext db, DateTime today)
    {
        if (!db.Members.Any())
        {
            db.Members.AddRange(
                new Member { Name = "Ayesha Khan", MemberNumber = "M-1001" },
                new Member { Name = "Daniel Brooks", MemberNumber = "M-1002" },
                new Member { Name = "Sara Malik", MemberNumber = "M-1003" });
        }

        var existing = db.TeeSlots.Select(s => s.StartTime).ToHashSet();
        for (var day = 0; day <= 8; day++)
        {
            for (var t = today.Date.AddDays(day).AddHours(6.5); t.Hour < 17; t = t.AddMinutes(10))
            {
                if (!existing.Contains(t)) db.TeeSlots.Add(new TeeSlot { StartTime = t });
            }
        }
        db.SaveChanges();
    }
}
