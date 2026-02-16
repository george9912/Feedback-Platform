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
            var entity = new Features.Feedback.Feedback(request.UserId, request.Rating, request.Comment);
            dbContext.Feedbacks.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            return new CreateFeedbackResponse(entity.Id);
        }
    }
}
