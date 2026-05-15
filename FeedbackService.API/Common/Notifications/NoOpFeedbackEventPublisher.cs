using FeedbackService.API.Common.Events;

namespace FeedbackService.API.Common.Notifications;

public sealed class NoOpFeedbackEventPublisher : IFeedbackEventPublisher
{
    private readonly ILogger<NoOpFeedbackEventPublisher> _logger;

    public NoOpFeedbackEventPublisher(ILogger<NoOpFeedbackEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishFeedbackCreatedAsync(FeedbackCreatedEvent evt, CancellationToken ct)
    {
        _logger.LogInformation(
            "Service Bus disabled. FeedbackCreated event not published for FeedbackId={FeedbackId}",
            evt.FeedbackId);

        return Task.CompletedTask;
    }
}