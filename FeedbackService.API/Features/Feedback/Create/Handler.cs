using FeedbackService.API.Infrastructure;
using static FeedbackService.API.Features.Feedback.Create.Command;

namespace FeedbackService.API.Features.Feedback.Create
{
    public sealed class Handler
    {
        private readonly AppDbContext dbContext;
        public Handler(AppDbContext db) => dbContext = db;

        public async Task<CreateFeedbackResponse> Handle(CreateFeedbackRequest request, CancellationToken ct = default)
        {
            var visibility = Enum.TryParse<Features.Feedback.FeedbackVisibility>(request.Visibility, true, out var parsed)
                ? parsed
                : Features.Feedback.FeedbackVisibility.Public;

            var entity = new Features.Feedback.Feedback(request.UserId, request.Rating, request.Comment, visibility, request.Tags);
            dbContext.Feedbacks.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            return new CreateFeedbackResponse(entity.Id);
        }
    }
}
