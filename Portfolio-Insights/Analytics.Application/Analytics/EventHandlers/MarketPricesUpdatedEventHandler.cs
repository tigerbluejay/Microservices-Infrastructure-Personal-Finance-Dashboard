using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Analytics.Application.Data;
using Analytics.Domain.Models;
using Analytics.Domain.ValueObjects;
using BuildingBlocks.Messaging.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Analytics.EventHandlers
{
    public interface IMarketPricesUpdatedEventHandler
    {
        Task Handle(MarketPricesUpdatedEvent @event, CancellationToken cancellationToken = default);
    }
    public class MarketPricesUpdatedEventHandler : IMarketPricesUpdatedEventHandler
    {
        private readonly IAnalyticsDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MarketPricesUpdatedEventHandler> _logger;

        public MarketPricesUpdatedEventHandler(
            IAnalyticsDbContext dbContext,
            IHttpClientFactory httpClientFactory,
            ILogger<MarketPricesUpdatedEventHandler> logger)
        {
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task Handle(MarketPricesUpdatedEvent @event, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[Analytics] Handling MarketPricesUpdatedEvent with {Count} updated prices", @event.Prices.Count);

            var priceDict = @event.Prices.ToDictionary(p => p.Symbol, p => p.Price);

            // Load all analytics records (one per user)
            var portfolios = await _dbContext.PortfolioAnalytics
                .Include(p => p.AssetContributions)
                .ToListAsync(cancellationToken);

            var http = _httpClientFactory.CreateClient("portfolio-service");

            foreach (var portfolio in portfolios)
            {
                try
                {
                    _logger.LogInformation("[Analytics] Using HttpClient BaseAddress: {BaseAddress}", http.BaseAddress);
                    // Fetch portfolio quantities for the user
                    var response = await http.GetAsync($"api/portfolio/{portfolio.User.Value}", cancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("[Analytics] Could not fetch portfolio for {UserName}: {StatusCode}", portfolio.User.Value, response.StatusCode);
                        continue;
                    }

                    var userPortfolio = await response.Content.ReadFromJsonAsync<UserPortfolioResponse>(cancellationToken: cancellationToken);
                    if (userPortfolio?.Assets == null || !userPortfolio.Assets.Any())
                    {
                        _logger.LogInformation("[Analytics] No assets found for {UserName}, skipping", portfolio.User.Value);
                        continue;
                    }

                    // Compute per-asset current values (quantity * price)
                    var assetValues = userPortfolio.Assets
                        .Where(a => priceDict.ContainsKey(a.Symbol))
                        .Select(a => (Symbol: a.Symbol, CurrentValue: a.Quantity * priceDict[a.Symbol]))
                        .ToList();

                    if (!assetValues.Any())
                    {
                        _logger.LogInformation("[Analytics] No matching price updates for {UserName}, skipping", portfolio.User.Value);
                        continue;
                    }

                    var previousTotal = portfolio.TotalValue;
                    var initialTotal = portfolio.Snapshots.FirstOrDefault()?.TotalValue;
                    
                    // Use domain logic to recompute analytics
                    portfolio.ComputeFromCurrentValues(assetValues, previousTotal, initialTotal);

                    _logger.LogInformation("[Analytics] Updated analytics for {UserName}. Total value: {TotalValue:F2}",
                        portfolio.User.Value, portfolio.TotalValue);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Analytics] Error while processing portfolio for {UserName}", portfolio.User.Value);
                }
            }
            
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[Analytics] Finished handling MarketPricesUpdatedEvent");
        }

        // Helper DTO for deserializing the Portfolio Service response
        private class UserPortfolioResponse
        {
            public string UserName { get; set; } = default!;
            public List<UserAssetDto> Assets { get; set; } = new();
        }

        private class UserAssetDto
        {
            public string Symbol { get; set; } = default!;
            public string Name { get; set; } = default!;
            public decimal Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal Value { get; set; }
        }
    }
}