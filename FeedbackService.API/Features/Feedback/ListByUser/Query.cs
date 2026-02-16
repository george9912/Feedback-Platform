namespace FeedbackService.API.Features.Feedback.ListByUser
{
    public class Query
    {
        public sealed record GetFeedbacksByUserRequest(Guid UserId, int Page = 1, int PageSize = 10);

        public sealed record GetFeedbacksByUserResponse(
            Guid Id,
            int Rating,
            string Comment,
            DateTime CreatedAt);
    }
}
