using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events;
using MarketData.Service.Data;
using MarketData.Service.Events;
using MarketData.Service.Models;
using MarketData.Service.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MarketData.Service.Handlers
{
    public class SimulateHandler : ICommandHandler<SimulateCommand, SimulateResult>
    {
        private readonly IMarketPriceRepository _marketRepo;
        private readonly IMarketPricesPublisher _publisher;
        private readonly MarketDataContext _context;

        private readonly Random _random = new();

        public SimulateHandler(
            IMarketPriceRepository marketRepo,
            IMarketPricesPublisher publisher,
            MarketDataContext context)
        {
            _marketRepo = marketRepo;
            _publisher = publisher;
            _context = context;
        }

        public async Task<SimulateResult> Handle(SimulateCommand request, CancellationToken cancellationToken)
        {
            var prices = await _marketRepo.GetAllAsync();

            var updatedPrices = new List<MarketPrice>();

            foreach (var price in prices)
            {
                // Fluctuate ±1–2%
                var factor = 1 + (decimal)(_random.NextDouble() * 0.02 - 0.01);
                price.Price *= factor;
                price.LastUpdated = DateTime.UtcNow;

                await _marketRepo.UpdateAsync(price);

                updatedPrices.Add(price);

                // Store log in SQLite
                _context.Logs.Add(new Logs
                {
                    Asset = price.Symbol,
                    Price = price.Price,
                    Timestamp = price.LastUpdated
                });

                // Console logging
                Console.WriteLine($"[Simulate] Updated {price.Symbol}: {price.Price:F2} at {price.LastUpdated:O}");
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Publish event
            var eventDtos = updatedPrices.Select(p => new MarketPriceDto(p.Symbol, p.Price)).ToList();
            await _publisher.PublishAsync(eventDtos);

            Console.WriteLine($"[Simulate] Published MarketPricesUpdatedEvent with {eventDtos.Count} prices.");

            return new SimulateResult(true, DateTime.UtcNow);
        }
    }
}