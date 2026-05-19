namespace FeedbackService.API.Features.Notifications;

public sealed class UserNotification
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Guid RecipientUserId { get; private set; }
    public Guid ActorUserId { get; private set; }
    public Guid FeedbackId { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ReadAtUtc { get; private set; }

    private UserNotification()
    {
    }

    public UserNotification(
        Guid eventId,
        Guid recipientUserId,
        Guid actorUserId,
        Guid feedbackId,
        string message,
        DateTime createdAtUtc)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        RecipientUserId = recipientUserId;
        ActorUserId = actorUserId;
        FeedbackId = feedbackId;
        Message = string.IsNullOrWhiteSpace(message)
            ? "You received new feedback."
            : message.Trim();
        IsRead = false;
        CreatedAtUtc = createdAtUtc;
    }

    public void MarkRead(DateTime readAtUtc)
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        ReadAtUtc = readAtUtc;
    }
}
