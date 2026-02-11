using CarRental.WebUI.Components;
using CarRental.WebUI.Server.Endpoints;
using CarRental.WebUI.Server.Extensions;
using CarRental.WebUI.Server.Middleware;
using CarRental.WebUI.Services;
using CarRental.Application;
using CarRental.Infrastructure;
using Microsoft.AspNetCore.Components;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddMudServices();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 

builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
        return new HttpClient
    {
        BaseAddress = new Uri(navigationManager.BaseUri)
    };
});
builder.Services.AddScoped<IApiClient, ApiClient>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

await app.InitialiseDatabaseAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); 
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseApiExceptionHandling();

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapAllEndpoints();


app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
