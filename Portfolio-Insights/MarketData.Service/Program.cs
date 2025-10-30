using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions;
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

// Add services to the container.

// Add services to the container
builder.Services.AddCarter();

var assembly = typeof(Program).Assembly;

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>)); // add validation behavior
    config.AddOpenBehavior(typeof(LoggingBehavior<,>)); // add logging behavior
});
builder.Services.AddValidatorsFromAssembly(assembly); // scans the assembly for validators and registers them
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

//if (builder.Environment.IsDevelopment())
//    builder.Services.InitializeMartenWith<CatalogInitialData>(); // seed data

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseMigration();
app.MapGrpcService<MarketDataGrpcService>();
app.UseExceptionHandler(options => { });
app.MapHealthChecks("/health",
    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.Run();
