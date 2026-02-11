using CarRental.Application.Cars.Queries;
using CarRental.Common.Dtos;
using CarRental.WebUI.Server.Contracts;
using MediatR;

namespace CarRental.WebUI.Server.Endpoints;

public sealed class CarEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/cars")
            .WithTags("Cars")
            .MapGet("/available", GetAvailableCarsAsync);
    }

    private static async Task<IResult> GetAvailableCarsAsync(
        string city,
        DateTime startDate,
        DateTime endDate,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var utcStartDate = NormalizeUtcDate(startDate);
        var utcEndDate = NormalizeUtcDate(endDate);
        var result = await sender.Send(new GetAvailableCarsQuery(city, utcStartDate, utcEndDate), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(ApiEnvelope<IReadOnlyList<CarDto>>.Ok(result.Value ?? []))
            : Results.BadRequest(ApiEnvelope<IReadOnlyList<CarDto>>.Fail(result.Error ?? "Unable to fetch cars."));
    }

    private static DateTime NormalizeUtcDate(DateTime dateTime) =>
        DateTime.SpecifyKind(dateTime.Date, DateTimeKind.Utc);
}
