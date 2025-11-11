using Carter;
using HealthChecks.UI.Client;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Service.Data;
using Portfolio.Service.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add all application services
builder.Services.AddApplicationServices(builder);

var app = builder.Build();

// Wipe & reseed database on startup (development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();

    await store.Advanced.Clean.CompletelyRemoveAllAsync();

    var seeder = new PortfolioInitialData();
    await seeder.Populate(store, default);

    Console.WriteLine("Portfolio database wiped and reseeded successfully.");
}

// Configure HTTP request pipeline
app.MapCarter();
app.UseExceptionHandler(options => { });

app.MapHealthChecks("/api/portfolio/health",
    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.Run();