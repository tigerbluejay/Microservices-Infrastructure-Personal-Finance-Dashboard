using Marten;
using Marten.Schema;
using Portfolio.Service.Models;

namespace Portfolio.Service.Data
{
    public class PortfolioInitialData : IInitialData
    {
        public async Task Populate(IDocumentStore store, CancellationToken cancellation)
        {
            using var session = store.LightweightSession();

            // Check if data already exists
            if (await session.Query<Portfolio.Service.Models.Portfolio>().AnyAsync(cancellation))
                return;

            var portfolios = GetPreconfiguredPortfolios();
            session.StoreObjects(portfolios);

            await session.SaveChangesAsync(cancellation);
        }

        private static IEnumerable<Portfolio.Service.Models.Portfolio> GetPreconfiguredPortfolios()
        {
            // Each Portfolio could represent a different user
            return new List<Portfolio.Service.Models.Portfolio>
            {
                new Portfolio.Service.Models.Portfolio
                {
                    Id = Guid.NewGuid(),
                    UserName = "johndoe",
                    Assets = new List<PortfolioAsset>
                    {
                        new PortfolioAsset { Id = Guid.NewGuid(), Symbol = "AAPL", Name = "Apple Inc.", Quantity = 15 },
                        new PortfolioAsset { Id = Guid.NewGuid(), Symbol = "GOOG", Name = "Alphabet Inc.", Quantity = 10 },
                        new PortfolioAsset { Id = Guid.NewGuid(), Symbol = "MSFT", Name = "Microsoft Corp.", Quantity = 20 },
                        new PortfolioAsset { Id = Guid.NewGuid(), Symbol = "AMZN", Name = "Amazon.com Inc.", Quantity = 5 },
                        new PortfolioAsset { Id = Guid.NewGuid(), Symbol = "META", Name = "Meta Platforms Inc.", Quantity = 8 }
                    }
                },
                new Portfolio.Service.Models.Portfolio
                {
                    Id = Guid.NewGuid(),
                    UserName = "janedoe",
                    Assets = new List<PortfolioAsset>
                    {
                        new PortfolioAsset { Id = Guid.NewGuid(), Symbol = "TSLA", Name = "Tesla Inc.", Quantity = 12 },
                        new PortfolioAsset { Id = Guid.NewGuid(), Symbol = "NFLX", Name = "Netflix Inc.", Quantity = 6 },
                        new PortfolioAsset { Id = Guid.NewGuid(), Symbol = "NVDA", Name = "NVIDIA Corp.", Quantity = 18 },
                        new PortfolioAsset { Id = Guid.NewGuid(), Symbol = "INTC", Name = "Intel Corp.", Quantity = 25 },
                        new PortfolioAsset { Id = Guid.NewGuid(), Symbol = "AMD", Name = "Advanced Micro Devices Inc.", Quantity = 22 }
                    }
                },
                new Portfolio.Service.Models.Portfolio
                {
                    Id = Guid.NewGuid(),
                    UserName = "bobsmith",
                    Assets = new List<PortfolioAsset>
                    {
                        new PortfolioAsset { Id = Guid.NewGuid(), Symbol = "JPM", Name = "JPMorgan Chase & Co.", Quantity = 14 },
                        new PortfolioAsset { Id = Guid.NewGuid(), Symbol = "XOM", Name = "Exxon Mobil Corp.", Quantity = 30 },
                        new PortfolioAsset { Id = Guid.NewGuid(), Symbol = "KO", Name = "Coca-Cola Co.", Quantity = 40 },
                        new PortfolioAsset { Id = Guid.NewGuid(), Symbol = "PEP", Name = "PepsiCo Inc.", Quantity = 35 },
                        new PortfolioAsset { Id = Guid.NewGuid(), Symbol = "DIS", Name = "Walt Disney Co.", Quantity = 10 }
                    }
                }
            };
        }
    }
}