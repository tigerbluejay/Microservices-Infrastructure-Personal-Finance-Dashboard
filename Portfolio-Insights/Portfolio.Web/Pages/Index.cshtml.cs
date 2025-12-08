using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Portfolio.Web.Models.Analytics;
using Portfolio.Web.Models.Notifications;
using Portfolio.Web.Services.Interfaces;

public class IndexModel : PageModel
{
    private readonly IAnalyticsService _analyticsService;
    private readonly INotificationService _notificationService;
    private readonly IMarketDataService _marketDataService;

    public IndexModel(
    IAnalyticsService analyticsService,
    INotificationService notificationService,
    IMarketDataService marketDataService)
    {
        _analyticsService = analyticsService;
        _notificationService = notificationService;
        _marketDataService = marketDataService;
    }

    public AnalyticsSummaryDto? Summary { get; private set; }
    public List<PortfolioHistorySnapshotDto> History { get; private set; } = new();
    public List<NotificationDto> LatestNotifications { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var user = "johndoe";

        try
        {
            Summary = await _analyticsService.GetSummaryAsync(user);
            History = await _analyticsService.GetHistoryAsync(user);
        }
        catch
        {
            Summary = null;
            History = new();
        }

        try
        {
            var notificationResponse =
                await _notificationService.GetNotificationsAsync(user, unread: false);

            LatestNotifications = notificationResponse.Notifications
                .OrderByDescending(x => x.CreatedAt)
                .Take(3)
                .ToList();
        }
        catch
        {
            LatestNotifications = new();
        }
    }

    public async Task<IActionResult> OnPostSimulateAsync()
    {
        var result = await _marketDataService.SimulateAsync();

        return new JsonResult(new
        {
            success = result.Success,
            timestamp = result.Timestamp
        });
    }
}