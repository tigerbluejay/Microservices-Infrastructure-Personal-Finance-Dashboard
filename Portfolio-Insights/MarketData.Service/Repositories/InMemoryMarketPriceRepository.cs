using MarketData.Service.Models;
using System.Collections.Concurrent;

namespace MarketData.Service.Repositories
{
    public class InMemoryMarketPriceRepository : IMarketPriceRepository
    {
        private readonly ConcurrentDictionary<string, MarketPrice> _prices = new();

        public InMemoryMarketPriceRepository()
        {
            // Seed with some dummy data
            _prices["AAPL"] = new MarketPrice { Symbol = "AAPL", Price = 172.35m, LastUpdated = DateTime.UtcNow };
            _prices["GOOG"] = new MarketPrice { Symbol = "GOOG", Price = 142.18m, LastUpdated = DateTime.UtcNow };
            _prices["MSFT"] = new MarketPrice { Symbol = "MSFT", Price = 318.67m, LastUpdated = DateTime.UtcNow };
            _prices["AMZN"] = new MarketPrice { Symbol = "AMZN", Price = 128.42m, LastUpdated = DateTime.UtcNow };
            _prices["META"] = new MarketPrice { Symbol = "META", Price = 308.15m, LastUpdated = DateTime.UtcNow };
            _prices["TSLA"] = new MarketPrice { Symbol = "TSLA", Price = 247.92m, LastUpdated = DateTime.UtcNow };
            _prices["NFLX"] = new MarketPrice { Symbol = "NFLX", Price = 390.56m, LastUpdated = DateTime.UtcNow };
            _prices["NVDA"] = new MarketPrice { Symbol = "NVDA", Price = 454.30m, LastUpdated = DateTime.UtcNow };
            _prices["INTC"] = new MarketPrice { Symbol = "INTC", Price = 34.85m, LastUpdated = DateTime.UtcNow };
            _prices["AMD"] = new MarketPrice { Symbol = "AMD", Price = 111.27m, LastUpdated = DateTime.UtcNow };
            _prices["JPM"] = new MarketPrice { Symbol = "JPM", Price = 152.48m, LastUpdated = DateTime.UtcNow };
            _prices["XOM"] = new MarketPrice { Symbol = "XOM", Price = 110.65m, LastUpdated = DateTime.UtcNow };
            _prices["KO"] = new MarketPrice { Symbol = "KO", Price = 58.74m, LastUpdated = DateTime.UtcNow };
            _prices["PEP"] = new MarketPrice { Symbol = "PEP", Price = 181.29m, LastUpdated = DateTime.UtcNow };
            _prices["DIS"] = new MarketPrice { Symbol = "DIS", Price = 90.83m, LastUpdated = DateTime.UtcNow };
        }

        public Task<MarketPrice?> GetAsync(string symbol)
        {
            _prices.TryGetValue(symbol, out var price);
            return Task.FromResult(price);
        }

        public Task<IEnumerable<MarketPrice>> GetAllAsync(IEnumerable<string> symbols)
        {
            var list = _prices.Where(p => symbols.Contains(p.Key)).Select(p => p.Value);
            return Task.FromResult(list);
        }

        public Task<IEnumerable<MarketPrice>> GetAllAsync()
        {
            return Task.FromResult(_prices.Values.AsEnumerable());
        }

        public Task UpdateAsync(MarketPrice price)
        {
            price.LastUpdated = DateTime.UtcNow;
            _prices[price.Symbol] = price;
            return Task.CompletedTask;
        }
    }
}