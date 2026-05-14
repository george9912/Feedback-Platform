namespace FeedbackService.API.Features.Feedback.GetById
{
    public class Query
    {
        public sealed record GetFeedbackByIdResponse(
                                                    Guid Id,
                                                    Guid UserId,
                                                    int Rating,
                                                    string Comment,
                                                    string Visibility,
                                                    string[] Tags,
                                                    DateTime CreatedAt);
    }
}
