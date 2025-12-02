using System;
using BuildingBlocks.CQRS;


namespace NotificationService.Notifications.Commands
{
    public record MarkNotificationAsReadCommand(Guid NotificationId) : ICommand;
}