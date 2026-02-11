using CarRental.Common.Dtos;
using CarRental.WebUI.Server.Contracts;

namespace CarRental.WebUI.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<CarDto>> GetAvailableCarsAsync(
        string city,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var utcStartDate = NormalizeUtcDate(startDate);
        var utcEndDate = NormalizeUtcDate(endDate);
        var url =
            $"/api/cars/available?city={Uri.EscapeDataString(city)}&startDate={Uri.EscapeDataString(utcStartDate.ToString("O"))}&endDate={Uri.EscapeDataString(utcEndDate.ToString("O"))}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await BuildClientExceptionAsync(response, cancellationToken);
        }

        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<List<CarDto>>>(cancellationToken);
        if (envelope is null)
        {
            throw new ApiClientException("Cars API returned an empty response.");
        }

        if (!envelope.Success || envelope.Data is null)
        {
            throw new ApiClientException(envelope.Error?.Message ?? "Unable to fetch available cars.", envelope.Error?.Errors);
        }

        return envelope.Data;
    }

    public async Task<int> CreateBookingAsync(
        int carId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var request = new CreateBookingApiRequest(
            carId,
            NormalizeUtcDate(startDate),
            NormalizeUtcDate(endDate));

        using var response = await _httpClient.PostAsJsonAsync(
            "/api/bookings",
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await BuildClientExceptionAsync(response, cancellationToken);
        }

        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<CreateBookingApiResponse>>(cancellationToken);
        if (envelope is null)
        {
            throw new ApiClientException("Booking API returned an empty response.");
        }

        if (!envelope.Success || envelope.Data is null)
        {
            throw new ApiClientException(envelope.Error?.Message ?? "Unable to create booking.", envelope.Error?.Errors);
        }

        return envelope.Data.BookingId;
    }

    private static async Task<ApiClientException> BuildClientExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var payload = await response.Content.ReadFromJsonAsync<ApiEnvelope<object>>(cancellationToken);
        var fallbackMessage = $"Request failed with status code {(int)response.StatusCode}.";
        var message = string.IsNullOrWhiteSpace(payload?.Error?.Message)
            ? fallbackMessage
            : payload.Error.Message;

        return new ApiClientException(message!, payload?.Error?.Errors);
    }

    private static DateTime NormalizeUtcDate(DateTime dateTime) =>
        DateTime.SpecifyKind(dateTime.Date, DateTimeKind.Utc);
}
