using FeedbackService.API.Infrastructure;
using Microsoft.EntityFrameworkCore;
using static FeedbackService.API.Features.Feedback.GetById.Query;

namespace FeedbackService.API.Features.Feedback.GetById
{
    public sealed class Handler
    {
        private readonly AppDbContext dbContext;
        public Handler(AppDbContext db) => dbContext = db;

        public async Task<GetFeedbackByIdResponse?> Handle(Guid id, CancellationToken ct = default)
        {
            return await dbContext.Feedbacks
                .Where(f => f.Id == id)
                .Select(f => new GetFeedbackByIdResponse(f.Id, f.UserId, f.Rating, f.Comment, f.CreatedAt))
                .FirstOrDefaultAsync(ct);
        }
    }
}
