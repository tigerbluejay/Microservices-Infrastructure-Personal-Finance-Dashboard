using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Analytics.Infrastructure.ExternalServices
{
    public class PortfolioApiClient
    {
        private readonly HttpClient _httpClient;

        public PortfolioApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PortfolioResponse?> GetPortfolioByUserAsync(string userName)
        {
            var response = await _httpClient.GetAsync($"/api/portfolios/{userName}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<PortfolioResponse>();
        }
    }

    public class PortfolioResponse
    {
        public string UserName { get; set; } = string.Empty;
        public List<PortfolioAsset> Assets { get; set; } = new();
        public decimal TotalValue { get; set; }
    }

    public class PortfolioAsset
    {
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Value { get; set; }
    }
}