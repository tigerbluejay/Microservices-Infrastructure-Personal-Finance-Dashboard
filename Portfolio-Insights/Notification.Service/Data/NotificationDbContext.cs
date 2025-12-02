using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using NotificationService.Models;

namespace NotificationService.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
        {
        }


        public DbSet<NotificationService.Models.Notification> Notifications { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<NotificationService.Models.Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.UserName).IsRequired();
                entity.Property(n => n.Message).IsRequired();
                entity.Property(n => n.CreatedAt).IsRequired();
                entity.Property(n => n.IsRead).HasDefaultValue(false);


                entity.HasIndex(n => n.UserName);
                entity.HasIndex(n => new { n.UserName, n.IsRead });
            });
        }
    }
}