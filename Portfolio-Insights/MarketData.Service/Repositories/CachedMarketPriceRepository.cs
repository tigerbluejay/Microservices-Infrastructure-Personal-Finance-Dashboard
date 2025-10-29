using MarketData.Service.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace MarketData.Service.Repositories
{
    // Proxy + Decorator pattern: Adds caching to any IMarketPriceRepository implementation
    public class CachedMarketPriceRepository(
        IMarketPriceRepository innerRepository,
        IDistributedCache cache
    ) : IMarketPriceRepository
    {
        private readonly DistributedCacheEntryOptions _cacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        public async Task<MarketPrice?> GetAsync(string symbol)
        {
            var cached = await cache.GetStringAsync(symbol);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonSerializer.Deserialize<MarketPrice>(cached);
            }

            var price = await innerRepository.GetAsync(symbol);
            if (price != null)
            {
                await cache.SetStringAsync(symbol, JsonSerializer.Serialize(price), _cacheOptions);
            }

            return price;
        }

        public async Task<IEnumerable<MarketPrice>> GetAllAsync(IEnumerable<string> symbols)
        {
            var result = new List<MarketPrice>();
            var symbolsToFetch = new List<string>();

            foreach (var symbol in symbols)
            {
                var cached = await cache.GetStringAsync(symbol);
                if (!string.IsNullOrEmpty(cached))
                {
                    result.Add(JsonSerializer.Deserialize<MarketPrice>(cached)!);
                }
                else
                {
                    symbolsToFetch.Add(symbol);
                }
            }

            if (symbolsToFetch.Any())
            {
                var fetched = await innerRepository.GetAllAsync(symbolsToFetch);
                foreach (var price in fetched)
                {
                    await cache.SetStringAsync(price.Symbol, JsonSerializer.Serialize(price), _cacheOptions);
                    result.Add(price);
                }
            }

            return result;
        }

        public async Task<IEnumerable<MarketPrice>> GetAllAsync()
        {
            var prices = await innerRepository.GetAllAsync();
            foreach (var price in prices)
            {
                await cache.SetStringAsync(price.Symbol, JsonSerializer.Serialize(price), _cacheOptions);
            }
            return prices;
        }

        public async Task UpdateAsync(MarketPrice price)
        {
            await innerRepository.UpdateAsync(price);
            await cache.SetStringAsync(price.Symbol, JsonSerializer.Serialize(price), _cacheOptions);
        }
    }
}