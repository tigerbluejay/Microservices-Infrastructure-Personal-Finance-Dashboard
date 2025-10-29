using MarketData.Service.Repositories;

namespace MarketData.Service.Services
{
    public class PriceUpdateService : BackgroundService
    {
        private readonly ILogger<PriceUpdateService> _logger;
        private readonly IMarketPriceRepository _repository;
        private readonly Random _random = new();

        public PriceUpdateService(
            ILogger<PriceUpdateService> logger,
            IMarketPriceRepository repository
            ) // optional
        {
            _logger = logger;
            _repository = repository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PriceUpdateService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var allPrices = await _repository.GetAllAsync(); // get all symbols

                    foreach (var price in allPrices)
                    {
                        // Random ±1–2% fluctuation
                        var changePercent = (_random.NextDouble() * 0.02) - 0.01;
                        price.Price *= (decimal)(1 + changePercent);
                        price.LastUpdated = DateTime.UtcNow;

                        await _repository.UpdateAsync(price);
                    }


                    _logger.LogInformation("Prices updated at {Time}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating prices");
                }

                // Wait 30 seconds
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}