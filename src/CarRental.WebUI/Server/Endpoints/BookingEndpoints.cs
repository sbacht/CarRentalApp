using CarRental.Application.Bookings.Commands;
using CarRental.WebUI.Server.Contracts;
using MediatR;

namespace CarRental.WebUI.Server.Endpoints;

public sealed class BookingEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/bookings")
            .WithTags("Bookings")
            .MapPost(string.Empty, CreateBookingAsync);
    }

    private static async Task<IResult> CreateBookingAsync(
        CreateBookingApiRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var utcStartDate = NormalizeUtcDate(request.StartDate);
        var utcEndDate = NormalizeUtcDate(request.EndDate);
        var result = await sender.Send(new CreateBookingCommand(request.CarId, utcStartDate, utcEndDate), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(ApiEnvelope<CreateBookingApiResponse>.Ok(new CreateBookingApiResponse(result.Value)))
            : Results.Conflict(ApiEnvelope<CreateBookingApiResponse>.Fail(result.Error ?? "Unable to create booking."));
    }

    private static DateTime NormalizeUtcDate(DateTime dateTime) =>
        DateTime.SpecifyKind(dateTime.Date, DateTimeKind.Utc);
}
