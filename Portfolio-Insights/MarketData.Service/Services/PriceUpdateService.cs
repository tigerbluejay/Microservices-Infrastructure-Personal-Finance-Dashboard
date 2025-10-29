using BuildingBlocks.Messaging.Events;
using MarketData.Service.Events;
using MarketData.Service.Repositories;

namespace MarketData.Service.Services
{
    public class PriceUpdateService : BackgroundService
    {
        private readonly ILogger<PriceUpdateService> _logger;
        private readonly IMarketPriceRepository _repository;
        private readonly IMarketPricesPublisher _marketPricesPublisher;
        private readonly Random _random = new();

        public PriceUpdateService(
            ILogger<PriceUpdateService> logger,
            IMarketPriceRepository repository,
            IMarketPricesPublisher marketPricesPublisher)
        {
            _logger = logger;
            _repository = repository;
            _marketPricesPublisher = marketPricesPublisher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PriceUpdateService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var allPrices = await _repository.GetAllAsync();

                    foreach (var price in allPrices)
                    {
                        // Random ±1–2% fluctuation
                        var changePercent = (_random.NextDouble() * 0.02) - 0.01;
                        price.Price *= (decimal)(1 + changePercent);
                        price.LastUpdated = DateTime.UtcNow;

                        await _repository.UpdateAsync(price);
                    }

                    _logger.LogInformation("Prices updated at {Time}", DateTime.UtcNow);

                    // ✅ Publish the updated prices to the message bus
                    var priceDtos = allPrices
                        .Select(p => new MarketPriceDto(p.Symbol, p.Price))
                        .ToList();

                    await _marketPricesPublisher.PublishAsync(priceDtos);

                    _logger.LogInformation("Published {Count} price updates.", priceDtos.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating or publishing prices");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}