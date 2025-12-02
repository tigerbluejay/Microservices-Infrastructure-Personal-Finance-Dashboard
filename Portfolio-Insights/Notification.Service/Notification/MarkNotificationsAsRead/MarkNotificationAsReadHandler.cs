using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Notifications.Commands;

namespace NotificationService.Notifications.Handlers
{
    public class MarkNotificationAsReadHandler
        : IRequestHandler<MarkNotificationAsReadCommand, Unit>
    {
        private readonly NotificationDbContext _context;

        public MarkNotificationAsReadHandler(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(
            MarkNotificationAsReadCommand request,
            CancellationToken cancellationToken)
        {
            var notification = await _context.Notifications
                .SingleOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken);

            if (notification == null)
                return Unit.Value; // idempotent

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }
    }
}