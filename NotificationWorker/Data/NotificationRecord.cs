namespace NotificationWorker.Data;

public sealed class NotificationRecord
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid FeedbackId { get; set; }
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public DateTime FeedbackCreatedAt { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
}
