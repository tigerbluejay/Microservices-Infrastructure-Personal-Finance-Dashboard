using System;


namespace NotificationService.Models
{
    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserName { get; set; } = default!;
        public string Message { get; set; } = default!;
        // Stored as UTC
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;


        // Optional — category/type for future UX/filtering
        public string? Type { get; set; }
    }
}