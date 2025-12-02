using System;


namespace NotificationService.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = default!;
        public string Message { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string? Type { get; set; }
    }
}