using MarketData.Service;

namespace Analytics.Infrastructure.Services
{
    public class MarketDataGrpcClient
    {
        private readonly MarketDataProtoService.MarketDataProtoServiceClient _client;

        public MarketDataGrpcClient(MarketDataProtoService.MarketDataProtoServiceClient client)
        {
            _client = client;
        }

        public async Task<decimal> GetPriceAsync(string symbol)
        {
            var request = new PriceRequest { Symbol = symbol };
            var response = await _client.GetPriceAsync(request);
            return (decimal)response.Price;
        }
    }
}