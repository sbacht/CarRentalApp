namespace CarRental.WebUI.Server.Contracts;

public sealed record CreateBookingApiRequest(int CarId, DateTime StartDate, DateTime EndDate);

public sealed record CreateBookingApiResponse(int BookingId);
