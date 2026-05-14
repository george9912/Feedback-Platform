namespace FeedbackService.API.Features.Feedback.ListByUser
{
    public class Query
    {
        public sealed record GetFeedbacksByUserRequest(
            Guid UserId,
            int Page = 1,
            int PageSize = 10,
            Guid? ViewerUserId = null,
            string? ViewerRole = null);

        public sealed record GetFeedbacksByUserResponse(
            Guid Id,
            int Rating,
            string Comment,
            string Visibility,
            string[] Tags,
            DateTime CreatedAt);
    }
}
