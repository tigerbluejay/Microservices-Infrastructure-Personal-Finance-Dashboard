using Portfolio.Web.Models.Notifications;
using Refit;

namespace Portfolio.Web.Services.Interfaces
{
    public interface INotificationService
    {
        // Temporary placeholder endpoint
        [Get("/health")]
        Task<string> HealthCheckAsync();

        //[Get("/notification-service/api/notifications/{userName}")]
        //Task<NotificationListResponseDto> GetNotificationsAsync(
        //    string userName,
        //    [Query] bool unread = false
        //    );

        [Get("/notification-service/api/notifications/{userName}")]
        Task<NotificationListResponseDto> GetNotificationsAsync(
            string userName,
            [Query] bool unread
        );

        [Post("/notification-service/api/notifications/{notificationId}/read")]
        Task MarkAsReadAsync(Guid notificationId);
    }
}
