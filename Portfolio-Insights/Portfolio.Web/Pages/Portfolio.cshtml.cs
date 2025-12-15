using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Portfolio.Web.Models.Analytics;
using Portfolio.Web.Models.Portfolio;
using Portfolio.Web.Services.Interfaces;
using System.Text.Json;

public class PortfolioModel : PageModel
{
    private readonly IPortfolioService _portfolioService;
    private readonly IMarketDataService _marketDataService;
    private readonly IAnalyticsService _analyticsService;

    private const string DemoUser = "johndoe";

    // JSON options for camelCase output
    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public PortfolioResponseDto Portfolio { get; private set; } = new();

    // Static list of available assets for dropdown
    public List<(string Symbol, string Name)> AvailableAssets { get; } = new()
    {
        ("AAPL","Apple Inc."),
        ("GOOG","Alphabet Inc."),
        ("MSFT","Microsoft Corp."),
        ("AMZN","Amazon.com Inc."),
        ("META","Meta Platforms Inc."),
        ("TSLA","Tesla Inc."),
        ("NFLX","Netflix Inc."),
        ("NVDA","Nvidia Corp."),
        ("INTC","Intel Corp."),
        ("AMD","AMD Inc."),
        ("JPM","JPMorgan Chase & Co."),
        ("XOM","Exxon Mobil Corp."),
        ("KO","Coca-Cola Co."),
        ("PEP","PepsiCo Inc."),
        ("DIS","Walt Disney Co.")
    };

    public PortfolioModel(IPortfolioService portfolioService, IMarketDataService marketdataservice, IAnalyticsService analyticsService)
    {
        _portfolioService = portfolioService;
        _marketDataService = marketdataservice;
        _analyticsService = analyticsService;
    }

    public async Task OnGetAsync()
    {
        try
        {
            Portfolio = await _portfolioService.GetPortfolioAsync(DemoUser);
        }
        catch
        {
            Portfolio = new PortfolioResponseDto();
        }
    }

    // Add asset via AJAX
    public async Task<IActionResult> OnPostAddAsync([FromBody] AddAssetRequestDto request)
    {
        try
        {
            await _portfolioService.AddAssetAsync(DemoUser, request);
            var updated = await _portfolioService.GetPortfolioAsync(DemoUser);

            return new JsonResult(updated, CamelCaseOptions);
        }
        catch (Exception ex)
        {
            return new JsonResult(new { error = ex.Message }, CamelCaseOptions)
            {
                StatusCode = 500
            };
        }
    }

    // Delete asset via AJAX
    public async Task<IActionResult> OnPostDeleteAsync(string symbol)
    {
        try
        {
            await _portfolioService.DeleteAssetAsync(DemoUser, symbol);
            var updated = await _portfolioService.GetPortfolioAsync(DemoUser);

            return new JsonResult(updated, CamelCaseOptions);
        }
        catch (Exception ex)
        {
            return new JsonResult(new { error = ex.Message }, CamelCaseOptions)
            {
                StatusCode = 500
            };
        }
    }

    public async Task<IActionResult> OnPostSimulateAsync()
    {
        try
        {
            // 1. Simulate market update
            await _marketDataService.SimulateAsync();

            // 2. Revalue portfolio
            await _portfolioService.RevalueAsync(DemoUser);

            // 3. Fetch latest portfolio
            var portfolio = await _portfolioService.GetPortfolioAsync(DemoUser);

            // 4. Build analytics refresh payload (IDENTICAL to Analytics page)
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

            // 5. Refresh analytics (THIS WAS MISSING)
            await _analyticsService.RefreshAnalyticsAsync(refreshRequest);

            // 6. Return updated portfolio to the page
            return new JsonResult(new
            {
                success = true,
                portfolio
            }, CamelCaseOptions);
        }
        catch (Exception ex)
        {
            return new JsonResult(new
            {
                success = false,
                error = ex.Message
            });
        }
    }

    public async Task<IActionResult> OnGetGetAsync()
    {
        var updated = await _portfolioService.GetPortfolioAsync(DemoUser);
        return new JsonResult(updated);
    }
}