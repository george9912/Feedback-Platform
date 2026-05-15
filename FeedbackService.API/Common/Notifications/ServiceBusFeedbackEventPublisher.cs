using Azure.Messaging.ServiceBus;
using FeedbackService.API.Common.Events;
using System.Text.Json;

namespace FeedbackService.API.Common.Notifications;

public sealed class ServiceBusFeedbackEventPublisher : IFeedbackEventPublisher
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusFeedbackEventPublisher> _logger;

    public ServiceBusFeedbackEventPublisher(
        ServiceBusClient client,
        IConfiguration configuration,
        ILogger<ServiceBusFeedbackEventPublisher> logger)
    {
        _logger = logger;

        var queueName = configuration["ServiceBus:QueueName"];
        if (string.IsNullOrWhiteSpace(queueName))
        {
            queueName = "feedback-created";
        }

        _sender = client.CreateSender(queueName);
    }

    public async Task PublishFeedbackCreatedAsync(FeedbackCreatedEvent evt, CancellationToken ct)
    {
        var msg = new ServiceBusMessage(JsonSerializer.Serialize(evt))
        {
            MessageId = evt.FeedbackId.ToString(),
            Subject = "FeedbackCreated",
            ContentType = "application/json"
        };

        try
        {
            await _sender.SendMessageAsync(msg, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Service Bus is unavailable. Skipping FeedbackCreated publish for FeedbackId={FeedbackId}", evt.FeedbackId);
        }
    }
}