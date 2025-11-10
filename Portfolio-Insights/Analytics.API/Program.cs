using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Http.Json;
using Analytics.API;
using Analytics.Application;
using Analytics.Infrastructure;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddAnalyticsApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApiServices(builder.Configuration);

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080); // HTTP
    options.ListenAnyIP(8081, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

var app = builder.Build();

app.MapHealthChecks("/health",
    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.UseApiServices();

app.Run();