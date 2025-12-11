using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Portfolio.Web.Models.Analytics;
using Portfolio.Web.Services.Interfaces;

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

    //    public async Task OnGetAsync()
    //    {
    //        try
    //        {
    //            // 1. Get latest portfolio
    //            var portfolio = await _portfolioService.GetPortfolioAsync(DemoUser);

    //            // 2. Prepare payload for refresh endpoint
    //            var refreshRequest = new RefreshAnalyticsRequestDto
    //            {
    //                UserName = DemoUser,
    //                Assets = portfolio.Assets.Select(a => new RefreshAnalyticsAssetDto
    //                {
    //                    Symbol = a.Symbol,
    //                    Quantity = a.Quantity,
    //                    CurrentPrice = a.Price
    //                }).ToList()
    //            };

    //            // 3. Recompute analytics in backend
    //            await _analyticsService.RefreshAnalyticsAsync(refreshRequest);

    //            // 4. Fetch updated analytics and history
    //            Analytics = await _analyticsService.GetSummaryAsync(DemoUser);
    //            History = await _analyticsService.GetHistoryAsync(DemoUser);
    //        }
    //        catch
    //        {
    //            // fallback: empty analytics to avoid breaking the page
    //            Analytics = new AnalyticsSummaryDto();
    //            History = new List<PortfolioHistorySnapshotDto>();
    //        }
    //    }

    //    public async Task<IActionResult> OnPostSimulateAsync()
    //    {
    //        try
    //        {
    //            // 1. Simulate market
    //            await _marketDataService.SimulateAsync();

    //            // 2. Revalue portfolio
    //            await _portfolioService.RevalueAsync(DemoUser);

    //            // 3. Fetch latest portfolio
    //            var portfolio = await _portfolioService.GetPortfolioAsync(DemoUser);

    //            // 4. Map portfolio to refresh payload
    //            var refreshRequest = new RefreshAnalyticsRequestDto
    //            {
    //                UserName = DemoUser,
    //                Assets = portfolio.Assets.Select(a => new RefreshAnalyticsAssetDto
    //                {
    //                    Symbol = a.Symbol,
    //                    Quantity = a.Quantity,
    //                    CurrentPrice = a.Price
    //                }).ToList()
    //            };

    //            // 5. Send to backend to recompute analytics
    //            await _analyticsService.RefreshAnalyticsAsync(refreshRequest);

    //            // 6. Fetch updated analytics and history
    //            var analytics = await _analyticsService.GetSummaryAsync(DemoUser);
    //            var history = await _analyticsService.GetHistoryAsync(DemoUser);

    //            return new JsonResult(new { analytics, history });
    //        }
    //        catch (Exception ex)
    //        {
    //            return new JsonResult(new { success = false, error = ex.Message });
    //        }
    //    }
    //}

    public async Task OnGetAsync()
    {
        try
        {
            // Always fetch portfolio and refresh analytics
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

            // Fetch updated analytics and history
            Analytics = await _analyticsService.GetSummaryAsync(DemoUser);
            History = await _analyticsService.GetHistoryAsync(DemoUser);
        }
        catch
        {
            Analytics = new AnalyticsSummaryDto();
            History = new List<PortfolioHistorySnapshotDto>();
        }
    }

    public async Task<IActionResult> OnPostSimulateAsync()
    {
        try
        {
            await _marketDataService.SimulateAsync();
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

            return new JsonResult(new { analytics, history });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = ex.Message });
        }
    }
}