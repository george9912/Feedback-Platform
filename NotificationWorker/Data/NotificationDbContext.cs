using Microsoft.EntityFrameworkCore;

namespace NotificationWorker.Data;

public sealed class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    public DbSet<NotificationRecord> Notifications => Set<NotificationRecord>();
    public DbSet<UserNotificationRecord> UserNotifications => Set<UserNotificationRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationRecord>(entity =>
        {
            entity.ToTable("ProcessedNotifications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CorrelationId).HasMaxLength(128);
            entity.HasIndex(x => x.EventId).IsUnique();
            entity.HasIndex(x => new { x.FeedbackId, x.ToUserId }).IsUnique();
        });

        modelBuilder.Entity<UserNotificationRecord>(entity =>
        {
            entity.ToTable("UserNotifications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Message).HasMaxLength(500).IsRequired();
            entity.HasIndex(x => x.EventId).IsUnique();
            entity.HasIndex(x => new { x.RecipientUserId, x.CreatedAtUtc });
            entity.HasIndex(x => new { x.RecipientUserId, x.IsRead });
        });
    }
}
