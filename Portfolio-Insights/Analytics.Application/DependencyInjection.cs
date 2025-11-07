using Analytics.Application.Analytics.EventHandlers;
using Analytics.Application.Services;
using MassTransit;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAnalyticsApplication(this IServiceCollection services)
        {
            // MediatR for commands, queries, and events
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

            // Register event handlers
            services.AddScoped<PortfolioUpdatedEventHandler>();
            services.AddScoped<MarketPricesUpdatedEventHandler>();

            // Register application services
            services.AddScoped<IAnalyticsCalculator, AnalyticsCalculator>();

            return services;
        }
    }
}