using FeedbackService.API.Common.Events;

namespace FeedbackService.API.Common.Notifications;

public interface IFeedbackEventPublisher
{
    Task PublishFeedbackCreatedAsync(FeedbackCreatedEvent evt, CancellationToken ct);
}