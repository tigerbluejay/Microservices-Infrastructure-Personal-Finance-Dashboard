using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.Events;
using BuildingBlocks.Messaging.DTOs;

namespace BuildingBlocks.Messaging.Events;

public record PortfolioUpdatedEvent(string UserName, List<PortfolioAssetDto> Assets) : IntegrationEvent;