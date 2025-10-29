using Grpc.Core;
using MarketData.Service.Repositories;

namespace MarketData.Service.Services
{
    public class MarketDataGrpcService : MarketDataProtoService.MarketDataProtoServiceBase
    {
        private readonly ILogger<MarketDataGrpcService> _logger;
        private readonly IMarketPriceRepository _repository;

        public MarketDataGrpcService(ILogger<MarketDataGrpcService> logger, IMarketPriceRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public override async Task<PriceReply> GetPrice(PriceRequest request, ServerCallContext context)
        {
            var price = await _repository.GetAsync(request.Symbol);
            if (price is null)
                throw new RpcException(new Status(StatusCode.NotFound, $"Symbol '{request.Symbol}' not found."));

            return new PriceReply
            {
                Symbol = price.Symbol,
                Price = (double)price.Price
            };
        }

        public override async Task<PriceListReply> GetPrices(PriceListRequest request, ServerCallContext context)
        {
            var prices = await _repository.GetAllAsync(request.Symbols);
            var reply = new PriceListReply();
            reply.Prices.AddRange(
                prices.Select(p => new PriceReply { Symbol = p.Symbol, Price = (double)p.Price })
            );
            return reply;
        }
    }
}