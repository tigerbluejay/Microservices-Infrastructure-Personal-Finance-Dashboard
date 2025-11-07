using System.Collections.Generic;
using MediatR;
using Analytics.Application.DTOs;

namespace Analytics.Application.Analytics.Queries
{
    public record GetAnalyticsHistoryQuery(string UserName) : IRequest<List<PortfolioAnalyticsSnapshotDTO>>;
}