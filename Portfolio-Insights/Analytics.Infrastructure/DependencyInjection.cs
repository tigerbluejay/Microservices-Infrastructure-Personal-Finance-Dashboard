using Analytics.Application.Data;
using Analytics.Application.EventPublishers;
using Analytics.Infrastructure.Data;
using Analytics.Infrastructure.Data.Extensions;
using Analytics.Infrastructure.Data.Interceptors;
using Analytics.Infrastructure.Messaging.Consumers;
using Analytics.Infrastructure.Messaging.Publishers;
using Analytics.Infrastructure.Services;
using BuildingBlocks.Messaging.Events;
using BuildingBlocks.Messaging.MassTransit;
using MarketData.Service;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Analytics.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Interceptors
            services.AddScoped<AuditableEntityInterceptor>();
            services.AddScoped<DispatchDomainEventInterceptor>();

            // Publishers
            services.AddScoped<IAnalyticsComputedPublisher, AnalyticsComputedPublisher>();
            services.AddScoped<AnalyticsComputedPublisher>();

            // Database
            services.AddDatabase(configuration);

            // gRPC Client
            services.AddScoped<MarketDataGrpcClient>();
            services.AddGrpcClient<MarketDataProtoService.MarketDataProtoServiceClient>(options =>
            {
                options.Address = new Uri(configuration["GrpcSettings:MarketDataUrl"]!);
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

            // Message broker setup (MassTransit)
            services.AddMessageBroker(configuration, Assembly.GetExecutingAssembly());

            // Add consumers to existing MassTransit registration
            services.TryAddScoped<PortfolioUpdatedConsumer>();
            services.TryAddScoped<MarketPricesUpdatedConsumer>();

            // Configure the bus if AddMessageBroker doesn't do it fully
            services.Configure<MassTransitHostOptions>(options =>
            {
                options.WaitUntilStarted = true;
                options.StartTimeout = TimeSpan.FromSeconds(30);
            });

            return services;
        }
    }
}