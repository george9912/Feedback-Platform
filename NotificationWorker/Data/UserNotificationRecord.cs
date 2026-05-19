namespace NotificationWorker.Data;

public sealed class UserNotificationRecord
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid RecipientUserId { get; set; }
    public Guid ActorUserId { get; set; }
    public Guid FeedbackId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ReadAtUtc { get; set; }
}
