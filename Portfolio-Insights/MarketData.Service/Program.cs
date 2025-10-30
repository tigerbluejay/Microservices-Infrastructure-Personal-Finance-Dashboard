using BuildingBlocks.Behaviors;
using BuildingBlocks.Messaging.MassTransit;
using Carter;
using FluentValidation;
using HealthChecks.UI.Client;
using MarketData.Service.Data;
using MarketData.Service.Events;
using MarketData.Service.Extensions;
using MarketData.Service.Repositories;
using MarketData.Service.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Very verbose logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Trace);

// Add services
builder.Services.AddCarter();
var assembly = typeof(Program).Assembly;

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(assembly);
builder.Services.AddGrpc();
builder.Services.AddMessageBroker(builder.Configuration);
builder.Services.AddSingleton<IMarketPriceRepository, InMemoryMarketPriceRepository>();
builder.Services.AddScoped<IMarketPricesPublisher, MarketPricesPublisher>();
builder.Services.Decorate<IMarketPriceRepository, CachedMarketPriceRepository>();
builder.Services.AddDbContext<MarketDataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Database")));
builder.Services.AddStackExchangeRedisCache(options =>
    options.Configuration = builder.Configuration.GetConnectionString("Redis"));
builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

var app = builder.Build();

// Exception handling middleware for **detailed logging**
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
app.UseMigration(); // ensure DB migrations are applied
app.MapGrpcService<MarketDataGrpcService>();
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapCarter();
app.Run();