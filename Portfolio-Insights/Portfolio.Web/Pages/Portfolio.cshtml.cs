using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Portfolio.Web.Models.Portfolio;
using Portfolio.Web.Services.Interfaces;
using System.Text.Json;

public class PortfolioModel : PageModel
{
    private readonly IPortfolioService _portfolioService;
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

    public PortfolioModel(IPortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
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

    // Revalue portfolio via AJAX
    public async Task<IActionResult> OnPostRevalueAsync()
    {
        try
        {
            // First call the revaluation endpoint (just updates prices)
            await _portfolioService.RevalueAsync(DemoUser);

            // Then load updated full portfolio
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

    public async Task<IActionResult> OnGetGetAsync()
    {
        var updated = await _portfolioService.GetPortfolioAsync(DemoUser);
        return new JsonResult(updated);
    }
}