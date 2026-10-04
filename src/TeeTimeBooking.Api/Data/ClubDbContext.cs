using Microsoft.EntityFrameworkCore;
using TeeTimeBooking.Api.Models;

namespace TeeTimeBooking.Api.Data;

public class ClubDbContext : DbContext
{
    public ClubDbContext(DbContextOptions<ClubDbContext> options) : base(options) { }

    public DbSet<Member> Members => Set<Member>();
    public DbSet<TeeSlot> TeeSlots => Set<TeeSlot>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TeeSlot>().HasIndex(s => new { s.Course, s.StartTime }).IsUnique();
        modelBuilder.Entity<Booking>().Property(b => b.Status).HasConversion<string>();
    }
}
