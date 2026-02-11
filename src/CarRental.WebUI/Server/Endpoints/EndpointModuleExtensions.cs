using System.Reflection;

namespace CarRental.WebUI.Server.Endpoints;

public static class EndpointModuleExtensions
{
    public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder app)
    {
        var endpointModules = Assembly.GetExecutingAssembly()
            .DefinedTypes
            .Where(type =>
                type is { IsAbstract: false, IsInterface: false } &&
                typeof(IEndpointModule).IsAssignableFrom(type))
            .Select(type => Activator.CreateInstance(type.AsType()) as IEndpointModule)
            .Where(module => module is not null)
            .Cast<IEndpointModule>();

        foreach (var module in endpointModules)
        {
            module.MapEndpoints(app);
        }

        return app;
    }
}
