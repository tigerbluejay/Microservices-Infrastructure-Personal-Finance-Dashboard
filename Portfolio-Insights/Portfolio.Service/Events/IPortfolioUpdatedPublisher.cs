using Portfolio.Service.DTOs;

namespace Portfolio.Service.Events
{
    public interface IPortfolioUpdatedPublisher
    {
        Task PublishAsync(string userName, List<PortfolioAssetDto> assets);
    }
}