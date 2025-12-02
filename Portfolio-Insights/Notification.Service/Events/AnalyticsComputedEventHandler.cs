using MediatR;
using NotificationService.Data;
using NotificationService.Models;

namespace NotificationService.Events
{
    public class AnalyticsComputedEventHandler
        : IRequestHandler<HandleAnalyticsComputedEvent, Unit>
    {
        private readonly NotificationDbContext _context;

        public AnalyticsComputedEventHandler(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(
            HandleAnalyticsComputedEvent request,
            CancellationToken cancellationToken)
        {
            var evt = request.Event;

            string? message = null;
            string? type = null;

            if (evt.DailyChangePercent >= 5m)
            {
                message = $"🎉 Your portfolio increased by {evt.DailyChangePercent:F2}% today.";
                type = "portfolio_gain";
            }
            else if (evt.DailyChangePercent <= -3m)
            {
                message = $"⚠️ Warning: your portfolio dropped by {Math.Abs(evt.DailyChangePercent):F2}% today.";
                type = "portfolio_loss";
            }

            if (message == null)
                return Unit.Value;

            _context.Notifications.Add(new NotificationService.Models.Notification
            {
                UserName = evt.UserName,
                Message = message,
                Type = type,
                CreatedAt = evt.ComputedAtUtc,
                IsRead = false
            });

            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}