using BuildingBlocks.CQRS;
using Portfolio.Service.DTOs;
using Portfolio.Service.Events;
using Portfolio.Service.Models;
using Portfolio.Service.Repositories;
using Portfolio.Service.Services;

namespace Portfolio.Service.Handlers
{
    public record AddAssetCommand(string UserName, CreatedAssetDto Asset) : ICommand<PortfolioValueDto>;

    public class AddAssetHandler : ICommandHandler<AddAssetCommand, PortfolioValueDto>
    {
        private readonly IPortfolioRepository _repository;
        private readonly MarketDataGrpcClient _marketData;
        private readonly IPortfolioUpdatedPublisher _publisher;

        public AddAssetHandler(
            IPortfolioRepository repository,
            MarketDataGrpcClient marketData,
            IPortfolioUpdatedPublisher publisher)
        {
            _repository = repository;
            _marketData = marketData;
            _publisher = publisher;
        }

        public async Task<PortfolioValueDto> Handle(AddAssetCommand request, CancellationToken cancellationToken)
        {
            var userName = request.UserName;
            var portfolio = await _repository.GetByUserNameAsync(userName);

            if (portfolio == null)
            {
                // if no portfolio exists, create one with the new asset
                var newPortfolio = new Portfolio.Service.Models.Portfolio
                {
                    Id = Guid.NewGuid(),
                    UserName = userName,
                    Assets = new List<PortfolioAsset>()
                };

                portfolio = newPortfolio;
            }

            var symbol = request.Asset.Symbol.ToUpperInvariant();
            var existingAsset = portfolio.Assets.FirstOrDefault(a => a.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));

            if (existingAsset != null)
            {
                // Preserve existing Id, update quantity (additive)
                existingAsset.Quantity += request.Asset.Quantity;
                existingAsset.Name = request.Asset.Name; // update name in case it changed
            }
            else
            {
                // create new asset with new Guid
                var asset = new PortfolioAsset
                {
                    Id = Guid.NewGuid(),
                    Symbol = symbol,
                    Name = request.Asset.Name,
                    Quantity = request.Asset.Quantity
                };

                portfolio.Assets.Add(asset);
            }

            // Save or update portfolio
            if (await _repository.GetByUserNameAsync(userName) == null)
            {
                await _repository.AddAsync(portfolio);
            }
            else
            {
                await _repository.UpdateAsync(portfolio);
            }

            // testing log
            var persisted = await _repository.GetByUserNameAsync(userName);

            if (persisted != null)
            {
                Console.WriteLine($"Portfolio for {userName} has {persisted.Assets.Count} assets.");
            }

            // Get price for the added/updated asset
            var prices = await _marketData.GetPricesAsync(new[] { symbol });
            prices.TryGetValue(symbol, out var price);
            var finalAsset = portfolio.Assets.First(a => a.Symbol == symbol);
            var value = finalAsset.Quantity * price;

            // Publish updated portfolio event (full list)
            var assetDtos = portfolio.Assets
                .Select(a => new BuildingBlocks.Messaging.DTOs.PortfolioAssetDto(a.Symbol, a.Quantity))
                .ToList();

            await _publisher.PublishAsync(userName, assetDtos);

            return new PortfolioValueDto(finalAsset.Symbol, finalAsset.Name, finalAsset.Quantity, price, value);
        }
    }
}