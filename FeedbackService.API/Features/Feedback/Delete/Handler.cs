using FeedbackService.API.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FeedbackService.API.Features.Feedback.Delete
{
    public sealed class Handler
    {
        private readonly AppDbContext _db;
        public Handler(AppDbContext db) => _db = db;

        public async Task<bool> Handle(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Feedbacks.FirstOrDefaultAsync(f => f.Id == id, ct);
            if (entity is null) return false;

            _db.Feedbacks.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
