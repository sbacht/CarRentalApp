using CarRental.Application.Common.Exceptions;
using CarRental.WebUI.Server.Contracts;

namespace CarRental.WebUI.Server.Middleware;

public sealed class ApiExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ApiExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex) when (IsApiRequest(context.Request.Path))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(ApiEnvelope<object>.Fail("Validation failed.", ex.Errors));
        }
        catch (Exception) when (IsApiRequest(context.Request.Path))
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(ApiEnvelope<object>.Fail("An unexpected server error occurred."));
        }
    }

    private static bool IsApiRequest(PathString path) => path.StartsWithSegments("/api");
}
