using BuildingBlocks.CQRS;
using Portfolio.Service.DTOs;
using Portfolio.Service.Events;
using Portfolio.Service.Models;
using Portfolio.Service.Repositories;

namespace Portfolio.Service.Handlers
{
    // Command
    public record CreateOrUpdatePortfolioCommand(string UserName, PortfolioDto Payload)
    : ICommand<CreateOrUpdatePortfolioResponse>;

    public class CreateOrUpdatePortfolioHandler
    : ICommandHandler<CreateOrUpdatePortfolioCommand, CreateOrUpdatePortfolioResponse>
    {
        private readonly IPortfolioRepository _repository;
        private readonly IPortfolioUpdatedPublisher _publisher;

        public CreateOrUpdatePortfolioHandler(IPortfolioRepository repository, IPortfolioUpdatedPublisher publisher)
        {
            _repository = repository;
            _publisher = publisher;
        }

        public async Task<CreateOrUpdatePortfolioResponse> Handle(CreateOrUpdatePortfolioCommand request, CancellationToken cancellationToken)
        {
            var existing = await _repository.GetByUserNameAsync(request.UserName);

            if (existing == null)
            {
                var newPortfolio = new Portfolio.Service.Models.Portfolio
                {
                    Id = Guid.NewGuid(),
                    UserName = request.UserName,
                    Assets = request.Payload.Assets.Select(a => new PortfolioAsset
                    {
                        Id = Guid.NewGuid(),
                        Symbol = a.Symbol.ToUpperInvariant(),
                        Name = a.Name,
                        Quantity = a.Quantity
                    }).ToList()
                };

                await _repository.AddAsync(newPortfolio);
                existing = newPortfolio;
            }
            else
            {
                var updatedAssets = new List<PortfolioAsset>();
                foreach (var incoming in request.Payload.Assets)
                {
                    var symbol = incoming.Symbol.ToUpperInvariant();
                    var found = existing.Assets.FirstOrDefault(a => a.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
                    if (found != null)
                    {
                        found.Name = incoming.Name;
                        found.Quantity = incoming.Quantity;
                        updatedAssets.Add(found);
                    }
                    else
                    {
                        updatedAssets.Add(new PortfolioAsset
                        {
                            Id = Guid.NewGuid(),
                            Symbol = symbol,
                            Name = incoming.Name,
                            Quantity = incoming.Quantity
                        });
                    }
                }

                existing.Assets = updatedAssets;
                await _repository.UpdateAsync(existing);
            }

            // publish event
            var assetDtos = existing.Assets
                .Select(a => new Portfolio.Service.DTOs.PortfolioAssetDto(a.Symbol, a.Quantity))
                .ToList();

            await _publisher.PublishAsync(existing.UserName, assetDtos);

            // return clean response (no meaningless zeros)
            var responseAssets = existing.Assets
                .Select(a => new PortfolioAssetSummaryDto(a.Symbol, a.Name, a.Quantity))
                .ToList();

            return new CreateOrUpdatePortfolioResponse(existing.UserName, responseAssets);
        }
    }
}