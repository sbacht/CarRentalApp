using AutoMapper;
using CarRental.Application.Bookings.Commands;
using CarRental.Application.Cars.Queries;
using CarRental.Application.Common.Mappings;
using CarRental.Common.Enums;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using CarRental.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace CarRental.Application.UnitTests;

public class ApplicationHandlersTests
{
    [Fact]
    public async Task GetAvailableCars_ShouldReturnOnlyCityCarsWithoutOverlaps_AndSetPrice()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var context = scope.Context;

        var start = DateTime.UtcNow.Date.AddDays(1);
        var end = start.AddDays(3);

        var availableCar = CreateCar("BMW", "3 Series", "BER001", "Berlin", 100m);
        var bookedCar = CreateCar("Audi", "A4", "BER002", "Berlin", 120m);
        var otherCityCar = CreateCar("VW", "Golf", "MUN001", "Munich", 90m);

        context.Cars.AddRange(availableCar, bookedCar, otherCityCar);
        await context.SaveChangesAsync(CancellationToken.None);

        context.Bookings.Add(new Booking(bookedCar.Id, new DateRange(start.AddDays(1), end.AddDays(1))));
        await context.SaveChangesAsync(CancellationToken.None);

        var mapper = CreateMapper();
        var handler = new GetAvailableCarsQueryHandler(context, mapper, new StubPriceService());

        var result = await handler.Handle(new GetAvailableCarsQuery("Berlin", start, end), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(1);
        result.Value[0].Id.Should().Be(availableCar.Id);
        result.Value[0].CarPrice.Should().Be(777m);
    }

    [Fact]
    public async Task CreateBooking_ShouldCreateBooking_WhenNoOverlapExists()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var context = scope.Context;

        var car = CreateCar("Tesla", "Model 3", "BER003", "Berlin", 150m);
        context.Cars.Add(car);
        await context.SaveChangesAsync(CancellationToken.None);

        var start = DateTime.UtcNow.Date.AddDays(2);
        var end = start.AddDays(2);
        var handler = new CreateBookingCommandHandler(context);

        var result = await handler.Handle(new CreateBookingCommand(car.Id, start, end), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeGreaterThan(0);

        var createdBooking = await context.Bookings.SingleAsync();
        createdBooking.CarId.Should().Be(car.Id);
        createdBooking.Period.Start.Should().Be(start);
        createdBooking.Period.End.Should().Be(end);
    }

    [Fact]
    public async Task CreateBooking_ShouldFail_WhenOverlappingBookingExists()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var context = scope.Context;

        var car = CreateCar("Mercedes", "C-Class", "BER004", "Berlin", 140m);
        context.Cars.Add(car);
        await context.SaveChangesAsync(CancellationToken.None);

        var existingStart = DateTime.UtcNow.Date.AddDays(5);
        var existingEnd = existingStart.AddDays(3);
        context.Bookings.Add(new Booking(car.Id, new DateRange(existingStart, existingEnd)));
        await context.SaveChangesAsync(CancellationToken.None);

        var overlapStart = existingStart.AddDays(1);
        var overlapEnd = existingEnd.AddDays(1);
        var handler = new CreateBookingCommandHandler(context);

        var result = await handler.Handle(new CreateBookingCommand(car.Id, overlapStart, overlapEnd), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Car is already booked for these dates.");
        (await context.Bookings.CountAsync()).Should().Be(1);
    }

    private static IMapper CreateMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(_ => { }, typeof(CarRentalProfile).Assembly);
        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IMapper>();
    }

    private static Car CreateCar(string brand, string model, string vinCode, string city, decimal basePrice)
    {
        return new Car(brand, model, vinCode, city, basePrice)
        {
            LicensePlate = $"{city[..2].ToUpperInvariant()}-1000",
            Year = 2024,
            TransmissionType = Transmission.Automatic,
            Fuel = FuelType.Electric,
            Class = CarClass.Comfort,
            SeatsCount = 5,
            CurrentMileage = 10_000
        };
    }

    private static DbContextOptions<TestApplicationDbContext> CreateSqliteOptions(string connectionString)
    {
        return new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseSqlite(connectionString)
            .Options;
    }

    private sealed class StubPriceService : IPriceService
    {
        public decimal CalculateCarPrice(decimal basePrice, DateTime start, DateTime end) => 777m;
    }
}

internal sealed class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Booking> Bookings => Set<Booking>();

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return Database.BeginTransactionAsync(cancellationToken);
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        return Database.CommitTransactionAsync(cancellationToken);
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        return Database.RollbackTransactionAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>().ComplexProperty(b => b.Period);
        modelBuilder.Entity<Car>().HasMany(c => c.Bookings).WithOne().HasForeignKey(b => b.CarId);
    }
}

internal sealed class TestDbContextScope : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    private TestDbContextScope(SqliteConnection connection, TestApplicationDbContext context)
    {
        _connection = connection;
        Context = context;
    }

    public TestApplicationDbContext Context { get; }

    public static async Task<TestDbContextScope> CreateAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new TestApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        return new TestDbContextScope(connection, context);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
