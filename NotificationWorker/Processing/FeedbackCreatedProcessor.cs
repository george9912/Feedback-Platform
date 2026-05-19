using Microsoft.EntityFrameworkCore;
using NotificationWorker.Common.Events;
using NotificationWorker.Data;

namespace NotificationWorker.Processing;

public sealed class FeedbackCreatedProcessor : IFeedbackCreatedProcessor
{
    private readonly NotificationDbContext _dbContext;
    private readonly ILogger<FeedbackCreatedProcessor> _logger;

    public FeedbackCreatedProcessor(
        NotificationDbContext dbContext,
        ILogger<FeedbackCreatedProcessor> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<FeedbackCreatedProcessResult> ProcessAsync(FeedbackCreatedEvent evt, CancellationToken ct)
    {
        var duplicateExists = await _dbContext.Notifications
            .AnyAsync(
                x => x.EventId == evt.EventId
                    || (x.FeedbackId == evt.FeedbackId && x.ToUserId == evt.ToUserId),
                ct);

        if (duplicateExists)
        {
            _logger.LogInformation(
                "Skipping duplicate notification event. EventId={EventId} FeedbackId={FeedbackId}",
                evt.EventId,
                evt.FeedbackId);

            return FeedbackCreatedProcessResult.Duplicate;
        }

        _dbContext.Notifications.Add(new NotificationRecord
        {
            Id = Guid.NewGuid(),
            EventId = evt.EventId,
            FeedbackId = evt.FeedbackId,
            FromUserId = evt.FromUserId,
            ToUserId = evt.ToUserId,
            FeedbackCreatedAt = evt.CreatedAt,
            CorrelationId = evt.CorrelationId,
            ProcessedAtUtc = DateTime.UtcNow
        });

        _dbContext.UserNotifications.Add(new UserNotificationRecord
        {
            Id = Guid.NewGuid(),
            EventId = evt.EventId,
            RecipientUserId = evt.ToUserId,
            ActorUserId = evt.FromUserId,
            FeedbackId = evt.FeedbackId,
            Message = "You received new feedback.",
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow,
            ReadAtUtc = null
        });

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Notification persisted. EventId={EventId} FeedbackId={FeedbackId} ToUserId={ToUserId}",
            evt.EventId,
            evt.FeedbackId,
            evt.ToUserId);

        return FeedbackCreatedProcessResult.Processed;
    }
}
