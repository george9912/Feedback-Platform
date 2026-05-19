using NotificationWorker.Common.Events;

namespace NotificationWorker.Processing;

public interface IFeedbackCreatedProcessor
{
    Task<FeedbackCreatedProcessResult> ProcessAsync(FeedbackCreatedEvent evt, CancellationToken ct);
}
