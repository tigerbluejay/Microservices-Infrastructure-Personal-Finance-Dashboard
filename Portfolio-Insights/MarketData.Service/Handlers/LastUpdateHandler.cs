using BuildingBlocks.CQRS;
using MarketData.Service.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketData.Service.Handlers
{
    // Query class for MediatR
    public record LastUpdateQuery() : IQuery<DateTime?>;

    // Handler for the query
    public class LastUpdateHandler : IQueryHandler<LastUpdateQuery, DateTime?>
    {
        private readonly MarketDataContext _context;

        public LastUpdateHandler(MarketDataContext context)
        {
            _context = context;
        }

        public async Task<DateTime?> Handle(LastUpdateQuery request, CancellationToken cancellationToken)
        {
            var last = await _context.Logs
                .AsNoTracking()
                .OrderByDescending(l => l.Timestamp)
                .ThenByDescending(l => l.Id)
                .Select(l => l.Timestamp)
                .FirstOrDefaultAsync(cancellationToken);

            return last == default ? null : last;
        }
    }
}