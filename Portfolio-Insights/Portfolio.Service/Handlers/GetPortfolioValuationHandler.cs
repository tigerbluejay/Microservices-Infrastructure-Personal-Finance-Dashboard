using BuildingBlocks.CQRS;
using Portfolio.Service.DTOs;
using Portfolio.Service.Repositories;
using Portfolio.Service.Services;

namespace Portfolio.Service.Handlers
{
    public record GetPortfolioValuationQuery(string UserName) : IQuery<List<PortfolioValueDto>>;

    public class GetPortfolioValuationHandler
        : IQueryHandler<GetPortfolioValuationQuery, List<PortfolioValueDto>>
    {
        private readonly IPortfolioRepository _repository;
        private readonly MarketDataGrpcClient _marketData;

        public GetPortfolioValuationHandler(
            IPortfolioRepository repository,
            MarketDataGrpcClient marketData)
        {
            _repository = repository;
            _marketData = marketData;
        }

        public async Task<List<PortfolioValueDto>> Handle(
            GetPortfolioValuationQuery request,
            CancellationToken cancellationToken)
        {
            var portfolio = await _repository.GetByUserNameAsync(request.UserName);
            if (portfolio == null)
                return new List<PortfolioValueDto>();

            var symbols = portfolio.Assets
                .Select(a => a.Symbol)
                .Distinct()
                .ToList();

            var prices = symbols.Any()
                ? await _marketData.GetPricesAsync(symbols)
                : new Dictionary<string, decimal>();

            return portfolio.Assets.Select(a =>
            {
                prices.TryGetValue(a.Symbol, out var price);
                var value = a.Quantity * price;

                return new PortfolioValueDto(
                    a.Symbol,
                    a.Name,
                    a.Quantity,
                    price,
                    value);
            }).ToList();
        }
    }
}