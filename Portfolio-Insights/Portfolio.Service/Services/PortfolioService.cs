using Portfolio.Service.DTOs;
using Portfolio.Service.Events;
using Portfolio.Service.Models;
using Portfolio.Service.Repositories;

namespace Portfolio.Service.Services
{
    public class PortfolioService
    {
        private readonly IPortfolioRepository _repository;
        private readonly MarketDataGrpcClient _marketDataClient;
        private readonly IPortfolioUpdatedPublisher _publisher;

        public PortfolioService(
            IPortfolioRepository repository,
            MarketDataGrpcClient marketDataClient,
            IPortfolioUpdatedPublisher publisher)
        {
            _repository = repository;
            _marketDataClient = marketDataClient;
            _publisher = publisher;
        }

        public async Task AddAssetAsync(string userName, PortfolioAssetDto assetDto)
        {
            var asset = new PortfolioAsset
            {
                Id = Guid.NewGuid(),
                Symbol = assetDto.Symbol,
                Name = assetDto.Name ?? "",
                Quantity = assetDto.Quantity
            };

            await _repository.AddAssetAsync(userName, asset);

            var portfolio = await _repository.GetByUserNameAsync(userName);
            var assetsDto = portfolio.Assets
                .Select(a => new PortfolioAssetDto(a.Symbol, a.Quantity, a.Name))
                .ToList();

            await _publisher.PublishAsync(userName, assetsDto);
        }

        public async Task RemoveAssetAsync(string userName, Guid assetId)
        {
            await _repository.RemoveAssetAsync(userName, assetId);

            var portfolio = await _repository.GetByUserNameAsync(userName);
            var assetsDto = portfolio.Assets
                .Select(a => new PortfolioAssetDto(a.Symbol, a.Quantity, a.Name))
                .ToList();

            await _publisher.PublishAsync(userName, assetsDto);
        }

        public async Task<List<PortfolioValueDto>> RevalueAsync(string userName)
        {
            var portfolio = await _repository.GetByUserNameAsync(userName);
            var symbols = portfolio.Assets.Select(a => a.Symbol);

            var prices = await _marketDataClient.GetPricesAsync(symbols);

            var result = portfolio.Assets.Select(a =>
                new PortfolioValueDto(
                    a.Symbol,
                    a.Name,
                    a.Quantity,
                    prices.ContainsKey(a.Symbol) ? prices[a.Symbol] : 0,
                    a.Quantity * (prices.ContainsKey(a.Symbol) ? prices[a.Symbol] : 0)
                )).ToList();

            // Optionally, publish updated portfolio event
            var assetsDto = portfolio.Assets.Select(a => new PortfolioAssetDto(a.Symbol, a.Quantity, a.Name)).ToList();
            await _publisher.PublishAsync(userName, assetsDto);

            return result;
        }
    }
}