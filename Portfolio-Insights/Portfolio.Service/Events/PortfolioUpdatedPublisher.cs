using MassTransit;
using Portfolio.Service.DTOs;

namespace Portfolio.Service.Events
{
    public class PortfolioUpdatedPublisher : IPortfolioUpdatedPublisher
    {
        private readonly IBus _bus;

        public PortfolioUpdatedPublisher(IBus bus)
        {
            _bus = bus;
        }

        public async Task PublishAsync(string userName, List<PortfolioAssetDto> assets)
        {
            var @event = new PortfolioUpdatedEvent(userName, assets);
            await _bus.Publish(@event);
        }
    }
}