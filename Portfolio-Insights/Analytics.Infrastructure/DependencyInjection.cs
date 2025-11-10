using Analytics.Application.Data;
using Analytics.Application.EventPublishers;
using Analytics.Infrastructure.Data;
using Analytics.Infrastructure.Data.Extensions;
using Analytics.Infrastructure.Data.Interceptors;
using Analytics.Infrastructure.Messaging.Consumers;
using Analytics.Infrastructure.Messaging.Publishers;
using Analytics.Infrastructure.Services;
using MarketData.Service;
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

            // Register your publisher
            services.AddScoped<IAnalyticsComputedPublisher, AnalyticsComputedPublisher>();

            services.AddDatabase(configuration);


            services.AddScoped<MarketDataGrpcClient>();
            services.AddScoped<AnalyticsComputedPublisher>();


            // gRPC Services
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


            services.AddMassTransit(cfg =>
            {
                cfg.AddConsumer<PortfolioUpdatedConsumer>();
                cfg.AddConsumer<MarketPricesUpdatedConsumer>();

                cfg.UsingRabbitMq((context, cfgBus) =>
                {
                    cfgBus.Host(new Uri(configuration["MessageBroker:Host"]!), h =>
                    {
                        h.Username(configuration["MessageBroker:UserName"]);
                        h.Password(configuration["MessageBroker:Password"]);
                    });

                    cfgBus.ConfigureEndpoints(context);
                });
            });

            // Add other infrastructure registrations (MassTransit, gRPC, etc.) in later phases
            return services;
        }
    }
}