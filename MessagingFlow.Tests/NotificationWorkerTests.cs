using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NotificationWorker.Common.Events;
using NotificationWorker.Data;
using NotificationWorker.Messaging;
using NotificationWorker.Processing;
using Xunit;

namespace MessagingFlow.Tests;

public sealed class NotificationWorkerTests
{
    [Fact]
    public async Task Processor_DuplicateEvents_DoNotCreateDuplicateNotifications()
    {
        var dbOptions = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new NotificationDbContext(dbOptions);
        var processor = new FeedbackCreatedProcessor(dbContext, NullLogger<FeedbackCreatedProcessor>.Instance);

        var evt = new FeedbackCreatedEvent
        {
            EventId = Guid.NewGuid(),
            FeedbackId = Guid.NewGuid(),
            FromUserId = Guid.NewGuid(),
            ToUserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString("N")
        };

        var first = await processor.ProcessAsync(evt, CancellationToken.None);
        var second = await processor.ProcessAsync(evt, CancellationToken.None);

        Assert.Equal(FeedbackCreatedProcessResult.Processed, first);
        Assert.Equal(FeedbackCreatedProcessResult.Duplicate, second);
        Assert.Single(dbContext.Notifications);
    }

    [Fact]
    public async Task Consumer_ProcessPayload_ConsumesFeedbackCreatedMessage()
    {
        var processor = new FakeProcessor();

        var services = new ServiceCollection();
        services.AddSingleton<IFeedbackCreatedProcessor>(processor);
        var provider = services.BuildServiceProvider();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var consumer = new RabbitMqFeedbackCreatedConsumer(
            configuration,
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<RabbitMqFeedbackCreatedConsumer>.Instance);

        var evt = new FeedbackCreatedEvent
        {
            EventId = Guid.NewGuid(),
            FeedbackId = Guid.NewGuid(),
            FromUserId = Guid.NewGuid(),
            ToUserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        var payload = JsonSerializer.Serialize(evt);

        var result = await consumer.ProcessPayloadAsync(payload, CancellationToken.None);

        Assert.Equal(FeedbackCreatedProcessResult.Processed, result);
        Assert.NotNull(processor.LastEvent);
        Assert.Equal(evt.EventId, processor.LastEvent!.EventId);
    }

    private sealed class FakeProcessor : IFeedbackCreatedProcessor
    {
        public FeedbackCreatedEvent? LastEvent { get; private set; }

        public Task<FeedbackCreatedProcessResult> ProcessAsync(FeedbackCreatedEvent evt, CancellationToken ct)
        {
            LastEvent = evt;
            return Task.FromResult(FeedbackCreatedProcessResult.Processed);
        }
    }
}
