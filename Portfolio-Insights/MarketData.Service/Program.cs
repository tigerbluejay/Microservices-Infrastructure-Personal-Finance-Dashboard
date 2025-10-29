using MarketData.Service.Data;
using MarketData.Service.Events;
using BuildingBlocks.Messaging.MassTransit;
using MarketData.Service.Extensions;
using MarketData.Service.Repositories;
using MarketData.Service.Services;
using HealthChecks.UI.Client;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddGrpc();
builder.Services.AddMessageBroker(builder.Configuration);
builder.Services.AddSingleton<IMarketPriceRepository, InMemoryMarketPriceRepository>();
builder.Services.AddScoped<IMarketPricesPublisher, MarketPricesPublisher>();
builder.Services.Decorate<IMarketPriceRepository, CachedMarketPriceRepository>();
builder.Services.AddDbContext<MarketDataContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Database"));
});
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseMigration();
app.MapGrpcService<MarketDataGrpcService>();

app.MapHealthChecks("/health",
    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.Run();
