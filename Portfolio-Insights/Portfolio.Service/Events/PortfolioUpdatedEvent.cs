using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.Events;
using Portfolio.Service.DTOs;

namespace Portfolio.Service.Events
{
    public record PortfolioUpdatedEvent(string UserName, List<PortfolioAssetDto> Assets) : IntegrationEvent;
}