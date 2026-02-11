namespace CarRental.WebUI.Services;

public sealed record ApiEnvelope<T>(
    bool Success,
    T? Data,
    ApiErrorResponse? Error);

public sealed record ApiErrorResponse(
    string? Message,
    IDictionary<string, string[]>? Errors = null);
