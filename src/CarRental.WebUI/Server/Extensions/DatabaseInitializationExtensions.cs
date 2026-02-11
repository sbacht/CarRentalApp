using CarRental.Infrastructure.Persistence;
using CarRental.Infrastructure.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;

namespace CarRental.WebUI.Server.Extensions;

public static class DatabaseInitializationExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await db.Database.EnsureCreatedAsync();
        await DbInitializer.SeedAsync(db);
    }
}
