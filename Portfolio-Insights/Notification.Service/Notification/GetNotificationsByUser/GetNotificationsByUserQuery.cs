using System.Collections.Generic;
using MediatR;
using NotificationService.DTOs;

namespace NotificationService.Notifications.Queries
{
    public record GetNotificationsByUserQuery(
        string UserName,
        bool UnreadOnly
    ) : IRequest<List<NotificationDto>>;
}