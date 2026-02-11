using CarRental.Common.Dtos;

namespace CarRental.WebUI.Services;

public interface IApiClient
{
    Task<IReadOnlyList<CarDto>> GetAvailableCarsAsync(string city, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<int> CreateBookingAsync(int carId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
