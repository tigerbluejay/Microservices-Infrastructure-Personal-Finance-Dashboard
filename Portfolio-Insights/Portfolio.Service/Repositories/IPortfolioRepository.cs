using Portfolio.Service.Models;

namespace Portfolio.Service.Repositories
{
    public interface IPortfolioRepository
    {
        Task<Portfolio.Service.Models.Portfolio?> GetByUserNameAsync(string userName);
        Task AddAsync(Portfolio.Service.Models.Portfolio portfolio);
        Task UpdateAsync(Portfolio.Service.Models.Portfolio portfolio);
        Task AddAssetAsync(string userName, PortfolioAsset asset);
        Task RemoveAssetBySymbolAsync(string userName, string symbol);
        Task SaveChangesAsync(); // optional, can be used for batching
    }
}

