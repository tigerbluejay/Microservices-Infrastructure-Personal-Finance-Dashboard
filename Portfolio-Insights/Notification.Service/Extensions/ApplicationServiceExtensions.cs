using BuildingBlocks.Messaging.MassTransit;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Data;
using System.Reflection;

namespace NotificationService.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration config)
        {
            // EF Core — SQLite
            services.AddDbContext<NotificationDbContext>(options =>
                options.UseSqlite(config.GetConnectionString("NotificationsDb")));

            // MediatR
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Carter
            services.AddCarter();

            // ✅ Message Broker (BuildingBlocks)
            services.AddMessageBroker(
                config,
                assembly: Assembly.GetExecutingAssembly());

            


            return services;
        }
    }
}