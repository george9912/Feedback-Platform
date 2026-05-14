namespace FeedbackService.API.Features.Feedback.Create
{
    public class Command
    {
        public record CreateFeedbackRequest(Guid UserId, int Rating, string Comment, string Visibility = "Public", string[]? Tags = null);
        public record CreateFeedbackResponse(Guid Id);
    }
}
