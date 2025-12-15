using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Portfolio.Web.Models.Notifications;
using Portfolio.Web.Services.Interfaces;

public class NotificationsModel : PageModel
{
    private readonly INotificationService _notificationService;

    public List<NotificationDto> AllNotifications { get; private set; } = new();
    public List<NotificationDto> UnreadNotifications { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string View { get; set; } = "all"; // all | unread

    public NotificationsModel(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task OnGetAsync()
    {
        var userName = User.Identity?.Name ?? "johndoe";

        var unreadTask = _notificationService
            .GetNotificationsAsync(userName, unread: true);

        var otherTask = _notificationService
            .GetNotificationsAsync(userName, unread: false);

        await Task.WhenAll(unreadTask, otherTask);

        UnreadNotifications = unreadTask.Result.Notifications;

        AllNotifications = unreadTask.Result.Notifications
            .Concat(otherTask.Result.Notifications)
            .GroupBy(n => n.Id)          //  remove duplicates
            .Select(g => g.First())
            .OrderByDescending(n => n.CreatedAt)
            .ToList();
    }

    public async Task<IActionResult> OnPostMarkAsReadAsync(Guid id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return RedirectToPage(new { View });
    }
}