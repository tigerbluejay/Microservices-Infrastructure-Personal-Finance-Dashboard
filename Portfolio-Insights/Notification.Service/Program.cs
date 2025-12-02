using Carter;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Extensions;
using HealthChecks.UI.Client;

var builder = WebApplication.CreateBuilder(args);

// ? ALL registrations live here
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// ? Apply migrations once, using the SAME DbContext
if (builder.Configuration.GetValue<bool>("ApplyMigrationsOnStart"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider
        .GetRequiredService<NotificationDbContext>();
    db.Database.Migrate();
}

// ? Carter endpoints
app.MapCarter();

// ? Health checks
app.MapHealthChecks("/api/notification/health",
    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.Run();