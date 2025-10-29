using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.Messaging.MassTransit
{
    public static class Extensions
    {
        public static IServiceCollection AddMessageBroker
            (this IServiceCollection services, IConfiguration configuration,
            Assembly? assembly = null)
        {
            // Implement RabbitMQ Mass Transit configuration here
            services.AddMassTransit(config =>
            {
                // Use kebab-case for endpoint names
                config.SetKebabCaseEndpointNameFormatter();

                // Automatically register all consumers from the specified assembly
                if (assembly != null)
                    config.AddConsumers(assembly);

                // Configure RabbitMQ as the message broker
                config.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(new Uri(configuration["MessageBroker:Host"]!), host =>
                    {
                        host.Username(configuration["MessageBroker:UserName"]);
                        host.Password(configuration["MessageBroker:Password"]);
                    });
                    // Configure endpoints for all registered consumers
                    cfg.ConfigureEndpoints(context);
                });
            });
            return services;
        }
    }
}