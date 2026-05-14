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
            var isOwner = request.ViewerUserId.HasValue && request.ViewerUserId.Value == request.UserId;
            var isHr = string.Equals(request.ViewerRole, "HR", StringComparison.OrdinalIgnoreCase);
            var isAdmin = string.Equals(request.ViewerRole, "ADMIN", StringComparison.OrdinalIgnoreCase);

            var query = _db.Feedbacks
                .Where(f => f.UserId == request.UserId);

            if (isHr || isAdmin)
            {
                // HR/Admin can review all feedback visibility levels.
            }
            else if (isOwner)
            {
                // Owners can see Public and Private feedback, not HR-only records.
                query = query.Where(f => f.Visibility != Features.Feedback.FeedbackVisibility.HROnly);
            }
            else
            {
                query = query.Where(f => f.Visibility == Features.Feedback.FeedbackVisibility.Public);
            }

            return await query
                .OrderByDescending(f => f.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(f => new GetFeedbacksByUserResponse(
                    f.Id,
                    f.Rating,
                    f.Comment,
                    f.Visibility.ToString(),
                    (f.Tags ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                    f.CreatedAt))
                .ToListAsync(ct);
        }
    }
}
