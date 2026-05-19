using FeedbackService.API.Infrastructure;
using FeedbackService.API.Common.Events;
using FeedbackService.API.Common.Notifications;
using System.Diagnostics;
using static FeedbackService.API.Features.Feedback.Create.Command;

namespace FeedbackService.API.Features.Feedback.Create
{
    public sealed class Handler
    {
        private readonly AppDbContext dbContext;
        private readonly IFeedbackEventPublisher eventPublisher;

        public Handler(AppDbContext db, IFeedbackEventPublisher eventPublisher)
        {
            dbContext = db;
            this.eventPublisher = eventPublisher;
        }

        public async Task<CreateFeedbackResponse> Handle(CreateFeedbackRequest request, CancellationToken ct = default)
        {
            var visibility = Enum.TryParse<Features.Feedback.FeedbackVisibility>(request.Visibility, true, out var parsed)
                ? parsed
                : Features.Feedback.FeedbackVisibility.Public;

            var entity = new Features.Feedback.Feedback(
                request.UserId,
                request.Rating,
                request.Comment,
                visibility,
                request.Tags,
                request.SubmittedByUserId);

            dbContext.Feedbacks.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var evt = new FeedbackCreatedEvent
            {
                EventId = Guid.NewGuid(),
                FeedbackId = entity.Id,
                FromUserId = entity.SubmittedByUserId ?? entity.UserId,
                ToUserId = entity.UserId,
                CreatedAt = entity.CreatedAt,
                CorrelationId = Activity.Current?.TraceId.ToString()
            };

            await eventPublisher.PublishFeedbackCreatedAsync(evt, ct);

            return new CreateFeedbackResponse(entity.Id);
        }
    }
}
