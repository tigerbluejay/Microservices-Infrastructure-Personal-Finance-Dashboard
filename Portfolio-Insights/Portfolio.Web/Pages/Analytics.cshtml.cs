using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Portfolio.Web.Models.Analytics;
using Portfolio.Web.Services.Interfaces;
using Refit;

public class AnalyticsModel : PageModel
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IPortfolioService _portfolioService;
    private readonly IMarketDataService _marketDataService;

    private const string DemoUser = "johndoe";

    public AnalyticsSummaryDto Analytics { get; private set; } = new();
    public List<PortfolioHistorySnapshotDto> History { get; private set; } = new();

    public AnalyticsModel(IAnalyticsService analyticsService, IPortfolioService portfolioService, IMarketDataService marketDataService)
    {
        _analyticsService = analyticsService;
        _portfolioService = portfolioService;
        _marketDataService = marketDataService;
    }

    //public async Task OnGetAsync()
    //{
    //    try
    //    {
    //        // Always fetch portfolio and refresh analytics
    //        var portfolio = await _portfolioService.GetPortfolioAsync(DemoUser);
    //        var refreshRequest = new RefreshAnalyticsRequestDto
    //        {
    //            UserName = DemoUser,
    //            Assets = portfolio.Assets.Select(a => new RefreshAnalyticsAssetDto
    //            {
    //                Symbol = a.Symbol,
    //                Quantity = a.Quantity,
    //                CurrentPrice = a.Price
    //            }).ToList()
    //        };

    //        await _analyticsService.RefreshAnalyticsAsync(refreshRequest);

    //        // Fetch updated analytics and history
    //        Analytics = await _analyticsService.GetSummaryAsync(DemoUser);
    //        History = await _analyticsService.GetHistoryAsync(DemoUser);
    //    }
    //    catch
    //    {
    //        Analytics = new AnalyticsSummaryDto();
    //        History = new List<PortfolioHistorySnapshotDto>();
    //    }
    //}

    //public async Task OnGetAsync()
    //{
    //    // 1️⃣ First attempt (may race RabbitMQ)
    //    Analytics = await _analyticsService.GetSummaryAsync(DemoUser);
    //    History = await _analyticsService.GetHistoryAsync(DemoUser);

    //    // 2️⃣ Small visual-stability delay (video-safe)
    //    await Task.Delay(300);

    //    // 3️⃣ If still empty, fetch again
    //    if (Analytics.TotalValue == 0 || History.Count == 0)
    //    {
    //        await Task.Delay(500); // optional extra cushion

    //        Analytics = await _analyticsService.GetSummaryAsync(DemoUser);
    //        History = await _analyticsService.GetHistoryAsync(DemoUser);
    //    }
    //}

    public async Task OnGetAsync()
    {
        const int maxAttempts = 3;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                Analytics = await _analyticsService.GetSummaryAsync(DemoUser);
                History = await _analyticsService.GetHistoryAsync(DemoUser);

                // success → exit immediately
                return;
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                if (attempt == maxAttempts)
                {
                    // Last attempt failed — show empty but SAFE page
                    Analytics = new AnalyticsSummaryDto();
                    History = new List<PortfolioHistorySnapshotDto>();
                    return;
                }

                // wait a bit and retry
                await Task.Delay(500);
            }
        }
    }

    public async Task<IActionResult> OnPostSimulateAsync()
    {
        try
        {
            var simulationResult = await _marketDataService.SimulateAsync();
            await _portfolioService.RevalueAsync(DemoUser);

            var portfolio = await _portfolioService.GetPortfolioAsync(DemoUser);
            var refreshRequest = new RefreshAnalyticsRequestDto
            {
                UserName = DemoUser,
                Assets = portfolio.Assets.Select(a => new RefreshAnalyticsAssetDto
                {
                    Symbol = a.Symbol,
                    Quantity = a.Quantity,
                    CurrentPrice = a.Price
                }).ToList()
            };

            await _analyticsService.RefreshAnalyticsAsync(refreshRequest);

            var analytics = await _analyticsService.GetSummaryAsync(DemoUser);
            var history = await _analyticsService.GetHistoryAsync(DemoUser);

            return new JsonResult(new
            {
                analytics,
                history,
                timestamp = simulationResult.Timestamp
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = ex.Message });
        }
    }
}