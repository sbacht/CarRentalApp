namespace CarRental.WebUI.Server.Middleware;

public static class ApiExceptionHandlingExtensions
{
    public static IApplicationBuilder UseApiExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ApiExceptionHandlingMiddleware>();
    }
}
