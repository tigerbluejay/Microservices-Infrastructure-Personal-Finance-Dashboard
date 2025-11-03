using MarketData.Service;

namespace Portfolio.Service.Services
{
    public class MarketDataGrpcClient
    {
        private readonly MarketDataProtoService.MarketDataProtoServiceClient _client;

        public MarketDataGrpcClient(MarketDataProtoService.MarketDataProtoServiceClient client)
        {
            _client = client;
        }

        public async Task<Dictionary<string, decimal>> GetPricesAsync(IEnumerable<string> symbols)
        {
            var request = new PriceListRequest();
            request.Symbols.AddRange(symbols);

            var response = await _client.GetPricesAsync(request);
            return response.Prices.ToDictionary(p => p.Symbol, p => (decimal)p.Price);
        }
    }
}
