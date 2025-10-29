using MarketData.Service.Data;
using MarketData.Service.Events;
using BuildingBlocks.Messaging.MassTransit;
using MarketData.Service.Extensions;
using MarketData.Service.Repositories;
using MarketData.Service.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddGrpc();
builder.Services.AddMessageBroker(builder.Configuration);
builder.Services.AddSingleton<IMarketPriceRepository, InMemoryMarketPriceRepository>();
builder.Services.AddScoped<IMarketPricesPublisher, MarketPricesPublisher>();
builder.Services.AddDbContext<MarketDataContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Database"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseMigration();
app.MapGrpcService<MarketDataGrpcService>();


app.Run();
