using Microsoft.EntityFrameworkCore;
using TeeTimeBooking.Api.Data;
using TeeTimeBooking.Api.Repositories;
using TeeTimeBooking.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ClubDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Club")));
builder.Services.AddScoped<ITeeTimeRepository, TeeTimeRepository>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClubDbContext>();
    db.Database.EnsureCreated();
    SeedData.Seed(db, DateTime.Today);
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapControllers();
app.Run();
