namespace CarRental.WebUI.Server.Contracts;

public sealed record ApiEnvelope<T>(bool Success, T? Data, ApiError? Error)
{
    public static ApiEnvelope<T> Ok(T data) => new(true, data, null);

    public static ApiEnvelope<T> Fail(string message, IDictionary<string, string[]>? errors = null) =>
        new(false, default, new ApiError(message, errors));
}

public sealed record ApiError(string? Message, IDictionary<string, string[]>? Errors = null);
