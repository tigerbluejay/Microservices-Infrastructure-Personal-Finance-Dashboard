using Marten;
using Portfolio.Service.Models;

namespace Portfolio.Service.Repositories
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly IDocumentSession _session;

        public PortfolioRepository(IDocumentSession session)
        {
            _session = session;
        }

        public async Task<Portfolio.Service.Models.Portfolio?> GetByUserNameAsync(string userName)
        {
            return await _session.Query<Portfolio.Service.Models.Portfolio>()
                                 .FirstOrDefaultAsync(p => p.UserName == userName);
        }

        public Task AddAsync(Portfolio.Service.Models.Portfolio portfolio)
        {
            _session.Store(portfolio);
            return _session.SaveChangesAsync();
        }

        public Task UpdateAsync(Portfolio.Service.Models.Portfolio portfolio)
        {
            _session.Store(portfolio);
            return _session.SaveChangesAsync();
        }

        public async Task AddAssetAsync(string userName, PortfolioAsset asset)
        {
            var portfolio = await GetByUserNameAsync(userName);
            if (portfolio == null) throw new Exception("Portfolio not found");
            portfolio.Assets.Add(asset);
            _session.Store(portfolio);
            await _session.SaveChangesAsync();
        }

        public async Task RemoveAssetAsync(string userName, Guid assetId)
        {
            var portfolio = await GetByUserNameAsync(userName);
            if (portfolio == null) throw new Exception("Portfolio not found");
            portfolio.Assets.RemoveAll(a => a.Id == assetId);
            _session.Store(portfolio);
            await _session.SaveChangesAsync();
        }

        public Task SaveChangesAsync() => _session.SaveChangesAsync();
    }
}
