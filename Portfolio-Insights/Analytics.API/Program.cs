using Analytics.API;
using Analytics.Application;
using Analytics.Infrastructure;
using Analytics.Infrastructure.Data;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;
using Grpc.Net.Client;
using BuildingBlocks.Services;
using MarketData.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddAnalyticsApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApiServices(builder.Configuration);

// Configure JSON enum serialization
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Explicit gRPC MarketData client registration (fixes DI issue)
builder.Services.AddGrpcClient<MarketDataProtoService.MarketDataProtoServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:MarketDataUrl"] ?? "http://marketdata.service:8080");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
    return handler;
});

builder.Services.AddScoped<MarketDataGrpcClient>(sp =>
{
    var protoClient = sp.GetRequiredService<MarketDataProtoService.MarketDataProtoServiceClient>();
    return new MarketDataGrpcClient(protoClient);
});

// Configure Kestrel endpoints
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080); // HTTP
    options.ListenAnyIP(8081, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

builder.Services.AddHttpClient("portfolio-service", client =>
{
    client.BaseAddress = new Uri("http://portfolio.service:8080/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// Health check endpoint
app.MapHealthChecks("/api/analytics/health",
    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

// Middleware & Carter endpoints
app.UseApiServices();

// Development-only seeding
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    DevSeeder.Seed(dbContext);
}

app.Run();