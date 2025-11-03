using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.MassTransit;
using Carter;
using HealthChecks.UI.Client;
using MarketData.Service;
using Marten;
using Portfolio.Service.Data;
using Weasel.Core;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var assembly = typeof(Program).Assembly;

// Application Services
builder.Services.AddCarter();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));

});

// Data Services
builder.Services.AddMarten(options =>
{
    // Connection string from appsettings.json
    options.Connection(builder.Configuration.GetConnectionString("Database")!);

    // Auto-create schema objects (tables, indexes) if missing
    options.AutoCreateSchemaObjects = AutoCreate.All;

    // Create the database automatically if it doesn't exist
    options.CreateDatabasesForTenants(c =>
    {
        c.ForTenant()
         .CheckAgainstPgDatabase()      // check if database exists
         .WithOwner("postgres")         // match your PostgreSQL user
         .WithEncoding("UTF-8")         // optional
         .ConnectionLimit(-1);          // optional
    });

    // Map the Portfolio document
    options.Schema.For<Portfolio.Service.Models.Portfolio>().Identity(x => x.Id);
})
.UseLightweightSessions();

// builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();


// Add gRPC Services
builder.Services.AddGrpcClient<MarketDataProtoService.MarketDataProtoServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:MarketDataUrl"]!);
})
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler
        {
            // Return `true` to allow certificates that are untrusted/invalid
            ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        return handler;

    });

// Async Communication Services
builder.Services.AddMessageBroker(builder.Configuration);

if (builder.Environment.IsDevelopment())
    builder.Services.InitializeMartenWith<PortfolioInitialData>(); // seed data

// Cross-cutting concerns
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Database")!);

var app = builder.Build();


// 
//  Wipe & reseed database on startup (development only)
// 
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();

    // 1? Wipe the Marten database completely
    // await store.Advanced.Clean.CompletelyRemoveAllAsync();

    // 2? Reseed data
    // var seeder = new PortfolioInitialData();
    // await seeder.Populate(store, default);

    Console.WriteLine("Catalog database wiped and reseeded successfully.");
}


// Configure the HTTP request pipeline.
app.MapCarter();
app.UseExceptionHandler(options => { });

app.MapHealthChecks("/health",
    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.Run();
