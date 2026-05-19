using FeedbackService.API.Common.Events;
using FeedbackService.API.Common.Notifications;
using FeedbackService.API.Features.Clients;
using FeedbackService.API.Features.Feedback.Create;
using FeedbackService.API.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MessagingFlow.Tests;

public sealed class FeedbackCreateHandlerTests
{
    [Fact]
    public async Task Handle_PersistsFeedback_AndPublishesFeedbackCreatedEvent()
    {
        var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AppDbContext(dbOptions);
        var publisher = new SpyPublisher(dbContext);
        var userClient = new FakeUserClient(userExists: true);
        var sut = new Handler(dbContext, publisher, userClient);

        var targetUserId = Guid.NewGuid();
        var fromUserId = Guid.NewGuid();
        var request = new Command.CreateFeedbackRequest(
            UserId: targetUserId,
            Rating: 5,
            Comment: "Great work",
            Visibility: "Public",
            Tags: ["kudos"],
            SubmittedByUserId: fromUserId);

        var result = await sut.Handle(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value!.Id);
        Assert.Single(dbContext.Feedbacks);
        Assert.Single(publisher.PublishedEvents);

        var published = publisher.PublishedEvents.Single();
        Assert.Equal(result.Value.Id, published.FeedbackId);
        Assert.Equal(targetUserId, published.ToUserId);
        Assert.Equal(fromUserId, published.FromUserId);
        Assert.True(publisher.FeedbackExistedAtPublish);
    }

    [Fact]
    public async Task Handle_WhenTargetUserMissing_ReturnsFailure_AndDoesNotPersist()
    {
        var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AppDbContext(dbOptions);
        var publisher = new SpyPublisher(dbContext);
        var userClient = new FakeUserClient(userExists: false);
        var sut = new Handler(dbContext, publisher, userClient);

        var request = new Command.CreateFeedbackRequest(
            UserId: Guid.NewGuid(),
            Rating: 4,
            Comment: "Test",
            Visibility: "Public",
            Tags: ["kudos"],
            SubmittedByUserId: Guid.NewGuid());

        var result = await sut.Handle(request, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Target user does not exist.", result.Error);
        Assert.Empty(dbContext.Feedbacks);
        Assert.Empty(publisher.PublishedEvents);
    }

    private sealed class SpyPublisher : IFeedbackEventPublisher
    {
        private readonly AppDbContext _dbContext;

        public SpyPublisher(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<FeedbackCreatedEvent> PublishedEvents { get; } = [];

        public bool FeedbackExistedAtPublish { get; private set; }

        public Task PublishFeedbackCreatedAsync(FeedbackCreatedEvent evt, CancellationToken ct)
        {
            FeedbackExistedAtPublish = _dbContext.Feedbacks.Any(x => x.Id == evt.FeedbackId);
            PublishedEvents.Add(evt);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserClient : IUserClient
    {
        private readonly bool _userExists;

        public FakeUserClient(bool userExists)
        {
            _userExists = userExists;
        }

        public Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_userExists);
        }
    }
}
