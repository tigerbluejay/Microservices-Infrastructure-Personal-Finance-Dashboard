using Carter;
using MediatR;
using MarketData.Service.Handlers;
using Microsoft.AspNetCore.Http;

namespace MarketData.Service.Endpoints
{
    public record LogsResponse(string Asset, decimal Price, DateTime Timestamp);

    public class LogsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/marketdata/logs", async (int? limit, ISender sender) =>
            {
                var query = new GetLogsQuery(limit ?? 10);
                var logs = await sender.Send(query);

                var response = logs.Select(l => new LogsResponse(l.Asset, l.Price, l.Timestamp)).ToList();

                return Results.Ok(response);
            })
            .WithName("GetMarketLogs")
            .Produces<List<LogsResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Retrieve latest market simulation logs")
            .WithDescription("Returns the most recent simulated market price updates stored in the database.");
        }
    }
}