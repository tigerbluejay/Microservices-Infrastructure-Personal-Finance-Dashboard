using Analytics.Infrastructure.Data;
using Analytics.Infrastructure.Data.Extensions;
using Analytics.Infrastructure.Data.Interceptors;
using Analytics.Infrastructure.Messaging.Consumers;
using Analytics.Infrastructure.Messaging.Publishers;
using Analytics.Infrastructure.Services;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<AuditableEntityInterceptor>();
            services.AddScoped<DispatchDomainEventInterceptor>();

            services.AddDatabase(configuration);

            services.AddScoped<MarketDataGrpcClient>();
            services.AddScoped<AnalyticsComputedPublisher>();

            // MassTransit consumers
            services.AddMassTransit(cfg =>
            {
                cfg.AddConsumer<PortfolioUpdatedConsumer>();
                cfg.AddConsumer<MarketPricesUpdatedConsumer>();
            });

            // Add other infrastructure registrations (MassTransit, gRPC, etc.) in later phases
            return services;
        }
    }
}