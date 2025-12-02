using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.DTOs;
using NotificationService.Notifications.Queries;

namespace NotificationService.Notifications.Handlers
{
    public class GetAllNotificationsByUserHandler
        : IRequestHandler<GetNotificationsByUserQuery, List<NotificationDto>>
    {
        private readonly NotificationDbContext _context;

        public GetAllNotificationsByUserHandler(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationDto>> Handle(
            GetNotificationsByUserQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserName == request.UserName);

            if (request.UnreadOnly)
                query = query.Where(n => !n.IsRead);

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    UserName = n.UserName,
                    Message = n.Message,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    Type = n.Type
                })
                .ToListAsync(cancellationToken);
        }
    }
}