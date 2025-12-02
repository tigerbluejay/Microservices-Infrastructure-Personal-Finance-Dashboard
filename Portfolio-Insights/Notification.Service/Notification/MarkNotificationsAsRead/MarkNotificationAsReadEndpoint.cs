using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using NotificationService.Notifications.Commands;

namespace NotificationService.Notifications.Endpoints
{
    public class MarkNotificationAsReadEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/notifications/{id}/read", async (
                Guid id,
                ISender sender
            ) =>
            {
                await sender.Send(new MarkNotificationAsReadCommand(id));
                return Results.NoContent();
            })
            .WithName("MarkNotificationAsRead")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Mark a notification as read")
            .WithDescription("Marks a specific notification as read. This operation is idempotent.");
        }
    }
}