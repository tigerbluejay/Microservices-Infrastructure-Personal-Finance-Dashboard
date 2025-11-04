using BuildingBlocks.CQRS;
using MediatR;
using Portfolio.Service.Events;
using Portfolio.Service.Repositories;

namespace Portfolio.Service.Handlers
{
    public record RemoveAssetCommand(string UserName, string Symbol) : ICommand;

    public class RemoveAssetHandler : ICommandHandler<RemoveAssetCommand>
    {
        private readonly IPortfolioRepository _repository;
        private readonly IPortfolioUpdatedPublisher _publisher;

        public RemoveAssetHandler(IPortfolioRepository repository, IPortfolioUpdatedPublisher publisher)
        {
            _repository = repository;
            _publisher = publisher;
        }

        public async Task<Unit> Handle(RemoveAssetCommand request, CancellationToken cancellationToken)
        {
            var symbol = request.Symbol.ToUpperInvariant();

            await _repository.RemoveAssetBySymbolAsync(request.UserName, symbol);

            // Fetch updated portfolio to publish event
            var portfolio = await _repository.GetByUserNameAsync(request.UserName);
            var assetDtos = portfolio?.Assets
                .Select(a => new Portfolio.Service.DTOs.PortfolioAssetDto(a.Symbol, a.Quantity))
                .ToList()
                ?? new List<Portfolio.Service.DTOs.PortfolioAssetDto>();

            await _publisher.PublishAsync(request.UserName, assetDtos);

            return Unit.Value;
        }
    }
}