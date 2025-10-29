using MarketData.Service.Models;

namespace MarketData.Service.Repositories
{
    public interface IMarketPriceRepository
    {
        Task<MarketPrice?> GetAsync(string symbol);
        Task<IEnumerable<MarketPrice>> GetAllAsync(IEnumerable<string> symbols);
        Task<IEnumerable<MarketPrice>> GetAllAsync();
        Task UpdateAsync(MarketPrice price);
    }
}