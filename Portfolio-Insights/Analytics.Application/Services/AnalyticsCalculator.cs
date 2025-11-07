using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Analytics.Domain.Models;
using Analytics.Domain.ValueObjects;
using BuildingBlocks.Messaging.DTOs;

namespace Analytics.Application.Services
{
    public class AnalyticsCalculator : IAnalyticsCalculator
    {
        public async Task<PortfolioAnalytics> ComputeAsync(
            string userName,
            List<PortfolioAssetDto> assets,
            Dictionary<string, decimal>? latestPrices = null)
        {
            return await Task.Run(() =>
            {
                if (assets == null || assets.Count == 0)
                    throw new ArgumentException("Asset list cannot be empty.", nameof(assets));

                // Step 1. Prepare asset values
                var assetValues = new List<(string Symbol, decimal CurrentValue)>();

                foreach (var asset in assets)
                {
                    decimal price = 0m;
                    if (latestPrices != null && latestPrices.TryGetValue(asset.Symbol, out var p))
                        price = p;

                    var value = asset.Quantity * price;
                    assetValues.Add((asset.Symbol, value));
                }

                // Step 2. Create aggregate
                var analytics = new PortfolioAnalytics(
                    new AnalyticsId(Guid.NewGuid()),
                    new UserName(userName)
                );

                // Step 3. Let the domain handle the actual computation logic
                analytics.ComputeFromCurrentValues(assetValues);

                return analytics;
            });
        }
    }
}