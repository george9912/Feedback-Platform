using FeedbackService.API.Common.Events;

namespace FeedbackService.API.Common.Notifications;

public sealed class NoOpCampaignNotificationDispatcher : ICampaignNotificationDispatcher
{
    private readonly ILogger<NoOpCampaignNotificationDispatcher> _logger;

    public NoOpCampaignNotificationDispatcher(ILogger<NoOpCampaignNotificationDispatcher> logger)
    {
        _logger = logger;
    }

    public Task DispatchAsync(CampaignNotificationEvent evt, DateTime? scheduledEnqueueTimeUtc, CancellationToken ct)
    {
        _logger.LogInformation(
            "Service Bus disabled. Campaign notification not dispatched. CampaignId={CampaignId}, Type={NotificationType}",
            evt.CampaignId,
            evt.NotificationType);

        return Task.CompletedTask;
    }
}