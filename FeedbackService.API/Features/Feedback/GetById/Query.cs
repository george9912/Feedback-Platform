namespace FeedbackService.API.Features.Feedback.GetById
{
    public class Query
    {
        public sealed record GetFeedbackByIdResponse(
                                                    Guid Id,
                                                    Guid UserId,
                                                    int Rating,
                                                    string Comment,
                                                    DateTime CreatedAt);
    }
}
