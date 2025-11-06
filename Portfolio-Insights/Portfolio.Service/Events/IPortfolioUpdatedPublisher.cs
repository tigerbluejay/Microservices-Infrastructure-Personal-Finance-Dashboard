using Portfolio.Service.DTOs;
using BuildingBlocks.Messaging.DTOs;

namespace Portfolio.Service.Events
{
    public interface IPortfolioUpdatedPublisher
    {
        Task PublishAsync(string userName, List<PortfolioAssetDto> assets);
    }
}