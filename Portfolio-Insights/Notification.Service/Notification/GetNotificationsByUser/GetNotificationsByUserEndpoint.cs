using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using NotificationService.DTOs;
using NotificationService.Notifications.Queries;

namespace NotificationService.Notifications.Endpoints
{
    public record GetNotificationsResponse(
        IReadOnlyList<NotificationDto> Notifications
    );

    public class GetNotificationsByUserEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/notifications/{user}", async (
                string user,
                bool unread,
                ISender sender
            ) =>
            {
                var query = new GetNotificationsByUserQuery(
                    UserName: user,
                    UnreadOnly: unread
                );

                var notifications = await sender.Send(query);
                return Results.Ok(new GetNotificationsResponse(notifications));
            })
            .WithName("GetNotificationsByUser")
            .Produces<GetNotificationsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Get notifications for a user")
            .WithDescription("Returns all notifications for a user, optionally filtered to unread only.");
        }
    }
}