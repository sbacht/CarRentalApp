using Microsoft.EntityFrameworkCore;
using CarRental.Common.Enums;
using CarRental.Domain.Entities;
using System.Text.Json;

namespace CarRental.Infrastructure.Persistence.Seeds
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Cars.AnyAsync()) return;

            var cities = new[] { "Berlin", "Munich", "Hamburg", "Frankfurt", "Stuttgart" };
            var brandsModels = new[]
            {
                ("VW", "Golf"),
                ("BMW", "3 Series"),
                ("Audi", "A4"),
                ("Tesla", "Model 3"),
                ("Mercedes", "C-Class")
            };

            var cars = new List<Car>();
            var random = new Random(42);
            var carIndex = 0;
            foreach (var city in cities)
            {
                foreach (var (brand, model) in brandsModels)
                {
                    carIndex++;
                    var basePrice = random.Next(40, 101);
                    var vinCode = $"VIN{city[..3].ToUpperInvariant()}{carIndex:00000000}";

                    var car = new Car(brand, model, vinCode, city, basePrice)
                    {
                        Year = random.Next(2019, DateTime.UtcNow.Year + 1),
                        LicensePlate = $"{city[..2].ToUpperInvariant()}-{1000 + carIndex}",
                        TransmissionType = random.Next(0, 2) == 0 ? Transmission.Manual : Transmission.Automatic,
                        Fuel = (FuelType)random.Next(0, Enum.GetValues<FuelType>().Length),
                        Class = (CarClass)random.Next(0, Enum.GetValues<CarClass>().Length),
                        SeatsCount = random.Next(0, 2) == 0 ? 4 : 5,
                        CurrentMileage = random.Next(5_000, 180_001),
                        FeaturesJson = JsonSerializer.Serialize(new Dictionary<string, string>
                        {
                            ["gps"] = (random.Next(2) == 0).ToString().ToLower(),
                            ["ac"] = (random.Next(2) == 0).ToString().ToLower(),
                            ["bluetooth"] = (random.Next(2) == 0).ToString().ToLower(),
                        })
                    };

                    cars.Add(car);
                }
            }

            await context.Cars.AddRangeAsync(cars);
            await context.SaveChangesAsync();
        }
    }
}
