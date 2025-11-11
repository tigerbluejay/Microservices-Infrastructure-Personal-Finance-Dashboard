using BuildingBlocks.Behaviors;
using BuildingBlocks.Messaging.MassTransit;
using Carter;
using FluentValidation;
using HealthChecks.UI.Client;
using MarketData.Service.Extensions;
using MarketData.Service.Models;
using MarketData.Service.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Verbose logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Trace);

// Add application services
builder.Services.AddApplicationServices(builder.Configuration);

// ===== Register Redis =====
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    // Use the Redis container name and port
    return ConnectionMultiplexer.Connect("distributedcache:6379");
});

var app = builder.Build();

// Exception handling middleware for detailed logging
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception caught in middleware");

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new
        {
            error = ex.Message,
            stackTrace = ex.StackTrace
        });
    }
});

app.UseHttpsRedirection();
app.UseMigration();
app.MapGrpcService<MarketDataGrpcService>();
app.MapHealthChecks("/api/marketdata/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapCarter();

// ===== Redis seeding =====
var redis = app.Services.GetRequiredService<IConnectionMultiplexer>().GetDatabase();

// Dummy market prices
var prices = new List<MarketPrice>
{
    new MarketPrice { Symbol = "AAPL", Price = 172.35m, LastUpdated = DateTime.UtcNow },
    new MarketPrice { Symbol = "GOOG", Price = 142.18m, LastUpdated = DateTime.UtcNow },
    new MarketPrice { Symbol = "MSFT", Price = 318.67m, LastUpdated = DateTime.UtcNow },
    new MarketPrice { Symbol = "AMZN", Price = 128.42m, LastUpdated = DateTime.UtcNow },
    new MarketPrice { Symbol = "META", Price = 308.15m, LastUpdated = DateTime.UtcNow },
    new MarketPrice { Symbol = "TSLA", Price = 247.92m, LastUpdated = DateTime.UtcNow },
    new MarketPrice { Symbol = "NFLX", Price = 390.56m, LastUpdated = DateTime.UtcNow },
    new MarketPrice { Symbol = "NVDA", Price = 454.30m, LastUpdated = DateTime.UtcNow },
    new MarketPrice { Symbol = "INTC", Price = 34.85m, LastUpdated = DateTime.UtcNow },
    new MarketPrice { Symbol = "AMD", Price = 111.27m, LastUpdated = DateTime.UtcNow },
    new MarketPrice { Symbol = "JPM", Price = 152.48m, LastUpdated = DateTime.UtcNow },
    new MarketPrice { Symbol = "XOM", Price = 110.65m, LastUpdated = DateTime.UtcNow },
    new MarketPrice { Symbol = "KO", Price = 58.74m, LastUpdated = DateTime.UtcNow },
    new MarketPrice { Symbol = "PEP", Price = 181.29m, LastUpdated = DateTime.UtcNow },
    new MarketPrice { Symbol = "DIS", Price = 90.83m, LastUpdated = DateTime.UtcNow }
};

// Store each price as JSON in Redis keyed by symbol
foreach (var price in prices)
{
    var json = JsonSerializer.Serialize(price);
    await redis.StringSetAsync($"MarketPrice:{price.Symbol}", json);
    Console.WriteLine($"Seeded Redis: {price.Symbol} = {price.Price}");
}

Console.WriteLine("Redis seeding complete.");
app.Run();