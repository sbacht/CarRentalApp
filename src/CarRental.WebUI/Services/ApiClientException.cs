namespace CarRental.WebUI.Services;

public class ApiClientException : Exception
{
    public ApiClientException(string message, IDictionary<string, string[]>? validationErrors = null)
        : base(message)
    {
        ValidationErrors = validationErrors;
    }

    public IDictionary<string, string[]>? ValidationErrors { get; }
}
