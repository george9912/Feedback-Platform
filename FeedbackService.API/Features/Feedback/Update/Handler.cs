using FeedbackService.API.Infrastructure;
using Microsoft.EntityFrameworkCore;
using static FeedbackService.API.Features.Feedback.Update.Command;

namespace FeedbackService.API.Features.Feedback.Update
{
    public sealed class Handler
    {
        private readonly AppDbContext _db;
        public Handler(AppDbContext db) => _db = db;

        public async Task<bool> Handle(Guid id, UpdateFeedbackRequest request, CancellationToken ct = default)
        {
            var entity = await _db.Feedbacks.FirstOrDefaultAsync(f => f.Id == id, ct);
            if (entity is null)
                return false;

            var visibility = Enum.TryParse<Features.Feedback.FeedbackVisibility>(request.Visibility, true, out var parsed)
                ? parsed
                : Features.Feedback.FeedbackVisibility.Public;

            entity.Update(request.Rating, request.Comment, visibility, request.Tags);

            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
