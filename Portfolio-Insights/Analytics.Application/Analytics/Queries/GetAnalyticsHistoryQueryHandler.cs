using Analytics.Application.Data;
using Analytics.Application.DTOs;
using Analytics.Application.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Analytics.Application.Analytics.Queries
{
    public class GetAnalyticsHistoryQueryHandler : IRequestHandler<GetAnalyticsHistoryQuery, List<PortfolioAnalyticsSnapshotDTO>>
    {
        private readonly IAnalyticsDbContext _dbContext;

        public GetAnalyticsHistoryQueryHandler(IAnalyticsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<PortfolioAnalyticsSnapshotDTO>> Handle(GetAnalyticsHistoryQuery request, CancellationToken cancellationToken)
        {
            var history = await _dbContext.PortfolioAnalytics
                .Where(a => a.User == request.UserName)
                .OrderByDescending(a => a.LastUpdatedUtc)
                .ToListAsync(cancellationToken);

            return history.Select(a => a.ToSnapshotDTO()).ToList();
        }
    }
}