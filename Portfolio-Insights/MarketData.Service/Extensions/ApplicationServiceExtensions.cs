using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.MassTransit;
using Carter;
using FluentValidation;
using MarketData.Service.Data;
using MarketData.Service.Events;
using MarketData.Service.Repositories;
using MarketData.Service.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MarketData.Service.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            var assembly = typeof(ApplicationServiceExtensions).Assembly;

            services.AddCarter();

            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(assembly);
                config.AddOpenBehavior(typeof(ValidationBehavior<,>));
                config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            });

            services.AddValidatorsFromAssembly(assembly);
            services.AddGrpc();
            services.AddMessageBroker(configuration);

            services.AddSingleton<IMarketPriceRepository, InMemoryMarketPriceRepository>();
            services.AddScoped<IMarketPricesPublisher, MarketPricesPublisher>();
            services.Decorate<IMarketPriceRepository, CachedMarketPriceRepository>();

            services.AddDbContext<MarketDataContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("Database")));

            services.AddStackExchangeRedisCache(options =>
                options.Configuration = configuration.GetConnectionString("Redis"));

            // Cross-cutting concerns
            services.AddExceptionHandler<CustomExceptionHandler>();
            services.AddHealthChecks()
                .AddRedis(configuration.GetConnectionString("Redis")!);

            return services;
        }
    }
}