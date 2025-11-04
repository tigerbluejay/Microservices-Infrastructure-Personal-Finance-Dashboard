using BuildingBlocks.CQRS;
using Portfolio.Service.DTOs;
using Portfolio.Service.Repositories;
using Portfolio.Service.Services;

namespace Portfolio.Service.Handlers
{
    // Query
    public record GetPortfolioQuery(string UserName) : IQuery<PortfolioDto>;

    public class GetPortfolioHandler : IQueryHandler<GetPortfolioQuery, PortfolioDto>
    {
        private readonly IPortfolioRepository _repository;
        private readonly MarketDataGrpcClient _marketData;

        public GetPortfolioHandler(IPortfolioRepository repository, MarketDataGrpcClient marketData)
        {
            _repository = repository;
            _marketData = marketData;
        }

        public async Task<PortfolioDto> Handle(GetPortfolioQuery request, CancellationToken cancellationToken)
        {
            var portfolio = await _repository.GetByUserNameAsync(request.UserName);
            if (portfolio == null) return null;

            var symbols = portfolio.Assets.Select(a => a.Symbol).Distinct().ToList();
            var prices = symbols.Any() ? await _marketData.GetPricesAsync(symbols) : new Dictionary<string, decimal>();

            var assets = portfolio.Assets.Select(a =>
            {
                prices.TryGetValue(a.Symbol, out var price);
                var value = a.Quantity * price;
                return new PortfolioValueDto(a.Symbol, a.Name, a.Quantity, price, value);
            }).ToList();

            var total = assets.Sum(a => a.Value);

            return new PortfolioDto(portfolio.UserName, assets, total);
        }
    }
}