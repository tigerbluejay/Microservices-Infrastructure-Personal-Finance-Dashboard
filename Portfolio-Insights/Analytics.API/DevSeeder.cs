using Analytics.Infrastructure.Data;
using Analytics.Domain.Models;
using Analytics.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Analytics.API
{
    public static class DevSeeder
    {
        public static void Seed(AnalyticsDbContext context)
        {
            context.Database.EnsureCreated();

            if (!context.PortfolioAnalytics.Any())
            {
                // Seed PortfolioAnalytics aggregates
                var portfolio1 = new PortfolioAnalytics(
                    new AnalyticsId(Guid.NewGuid()),
                    new UserName("johndoe")
                );
                portfolio1.ComputeFromCurrentValues(new List<(string Symbol, decimal CurrentValue)>
                {
                    ("AAPL", 1000m),
                    ("GOOGL", 1500m),
                    ("TSLA", 500m)
                }, previousTotalValue: 2700m, initialTotalValue: 2500m);

                var portfolio2 = new PortfolioAnalytics(
                    new AnalyticsId(Guid.NewGuid()),
                    new UserName("janedoe")
                );
                portfolio2.ComputeFromCurrentValues(new List<(string Symbol, decimal CurrentValue)>
                {
                    ("MSFT", 2000m),
                    ("AMZN", 1000m)
                }, previousTotalValue: 2800m, initialTotalValue: 2600m);

                context.PortfolioAnalytics.AddRange(portfolio1, portfolio2);

                context.SaveChanges();
            }
        }
    }
}