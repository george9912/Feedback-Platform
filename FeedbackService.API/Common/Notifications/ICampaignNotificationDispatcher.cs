using FeedbackService.API.Common.Events;

namespace FeedbackService.API.Common.Notifications;

public interface ICampaignNotificationDispatcher
{
    Task DispatchAsync(CampaignNotificationEvent evt, DateTime? scheduledEnqueueTimeUtc, CancellationToken ct);
}
