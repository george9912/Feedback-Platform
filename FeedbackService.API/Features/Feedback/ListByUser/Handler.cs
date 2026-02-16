using FeedbackService.API.Infrastructure;
using Microsoft.EntityFrameworkCore;
using static FeedbackService.API.Features.Feedback.ListByUser.Query;

namespace FeedbackService.API.Features.Feedback.ListByUser
{
    public sealed class Handler
    {
        private readonly AppDbContext _db;
        public Handler(AppDbContext db) => _db = db;

        public async Task<IEnumerable<GetFeedbacksByUserResponse>> Handle(GetFeedbacksByUserRequest request, CancellationToken ct = default)
        {
            return await _db.Feedbacks
                .Where(f => f.UserId == request.UserId)
                .OrderByDescending(f => f.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(f => new GetFeedbacksByUserResponse(f.Id, f.Rating, f.Comment, f.CreatedAt))
                .ToListAsync(ct);
        }
    }
}
