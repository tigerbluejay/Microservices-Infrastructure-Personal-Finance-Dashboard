using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.MassTransit;
using Carter;
using HealthChecks.UI.Client;
using MarketData.Service;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Service.Data;
using Portfolio.Service.Events;
using Portfolio.Service.Repositories;
using Portfolio.Service.Services;
using System.Reflection;
using Weasel.Core;

namespace Portfolio.Service.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, WebApplicationBuilder builder)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Application Services
            services.AddCarter();
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(assembly);
                config.AddOpenBehavior(typeof(ValidationBehavior<,>));
                config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            });

            // Data Services (Marten)
            services.AddMarten(options =>
            {
                options.Connection(builder.Configuration.GetConnectionString("Database")!);
                options.AutoCreateSchemaObjects = AutoCreate.All;

                options.CreateDatabasesForTenants(c =>
                {
                    c.ForTenant()
                     .CheckAgainstPgDatabase()
                     .WithOwner("postgres")
                     .WithEncoding("UTF-8")
                     .ConnectionLimit(-1);
                });

                options.Schema.For<Portfolio.Service.Models.Portfolio>().Identity(x => x.Id);
            })
            .UseLightweightSessions();

            // gRPC Services
            services.AddGrpcClient<MarketDataProtoService.MarketDataProtoServiceClient>(options =>
            {
                options.Address = new Uri(builder.Configuration["GrpcSettings:MarketDataUrl"]!);
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

            // Wrapper gRPC client
            services.AddScoped<Portfolio.Service.Services.MarketDataGrpcClient>();

            // Async Communication
            services.AddMessageBroker(builder.Configuration);

            // Portfolio Repository
            services.AddScoped<IPortfolioRepository, PortfolioRepository>();

            // Portfolio Services & Event Publisher
            services.AddScoped<IPortfolioUpdatedPublisher, PortfolioUpdatedPublisher>();
            services.AddScoped<PortfolioService>();

            // Seed data (Development only)
            if (builder.Environment.IsDevelopment())
                services.InitializeMartenWith<PortfolioInitialData>();

            // Cross-cutting concerns
            services.AddExceptionHandler<CustomExceptionHandler>();
            services.AddHealthChecks()
                    .AddNpgSql(builder.Configuration.GetConnectionString("Database")!);

            return services;
        }
    }
}