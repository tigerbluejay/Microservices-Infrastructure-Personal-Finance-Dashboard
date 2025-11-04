using BuildingBlocks.CQRS;
using Portfolio.Service.DTOs;
using Portfolio.Service.Repositories;
using Portfolio.Service.Services;

namespace Portfolio.Service.Handlers
{
    public record RevaluePortfolioCommand(string UserName) : ICommand<List<PortfolioValueDto>>;

    public class RevaluePortfolioHandler : ICommandHandler<RevaluePortfolioCommand, List<PortfolioValueDto>>
    {
        private readonly IPortfolioRepository _repository;
        private readonly MarketDataGrpcClient _marketData;

        public RevaluePortfolioHandler(IPortfolioRepository repository, MarketDataGrpcClient marketData)
        {
            _repository = repository;
            _marketData = marketData;
        }

        public async Task<List<PortfolioValueDto>> Handle(RevaluePortfolioCommand request, CancellationToken cancellationToken)
        {
            var portfolio = await _repository.GetByUserNameAsync(request.UserName);
            if (portfolio == null) return new List<PortfolioValueDto>();

            var symbols = portfolio.Assets.Select(a => a.Symbol).Distinct().ToList();
            var prices = symbols.Any() ? await _marketData.GetPricesAsync(symbols) : new Dictionary<string, decimal>();

            var result = portfolio.Assets.Select(a =>
            {
                prices.TryGetValue(a.Symbol, out var price);
                var value = a.Quantity * price;
                return new PortfolioValueDto(a.Symbol, a.Name, a.Quantity, price, value);
            }).ToList();

            return result;
        }
    }
}