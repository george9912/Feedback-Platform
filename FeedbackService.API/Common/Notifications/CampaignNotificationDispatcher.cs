using Azure.Messaging.ServiceBus;
using FeedbackService.API.Common.Events;
using System.Text.Json;

namespace FeedbackService.API.Common.Notifications;

public class CampaignNotificationDispatcher : ICampaignNotificationDispatcher
{
    private readonly ServiceBusClient _client;
    private readonly string _queueName;
    private readonly ILogger<CampaignNotificationDispatcher> _logger;

    public CampaignNotificationDispatcher(
        ServiceBusClient client,
        IConfiguration configuration,
        ILogger<CampaignNotificationDispatcher> logger)
    {
        _client = client;
        _logger = logger;
        _queueName = configuration["ServiceBus:CampaignNotificationQueueName"] ?? "campaign-notifications";
    }

    public async Task DispatchAsync(CampaignNotificationEvent evt, DateTime? scheduledEnqueueTimeUtc, CancellationToken ct)
    {
        var sender = _client.CreateSender(_queueName);

        var message = new ServiceBusMessage(JsonSerializer.Serialize(evt))
        {
            MessageId = $"{evt.CampaignId}:{evt.NotificationType}:{evt.TriggeredAtUtc:yyyyMMddHHmmss}",
            Subject = evt.NotificationType,
            ContentType = "application/json"
        };

        if (scheduledEnqueueTimeUtc.HasValue)
        {
            message.ScheduledEnqueueTime = scheduledEnqueueTimeUtc.Value;
        }

        try
        {
            await sender.SendMessageAsync(message, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Service Bus is unavailable. Skipping campaign notification {NotificationType} for CampaignId={CampaignId}",
                evt.NotificationType,
                evt.CampaignId);
        }
    }
}
