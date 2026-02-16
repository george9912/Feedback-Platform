namespace FeedbackService.API.Features.Feedback.Create
{
    public class Command
    {
        public record CreateFeedbackRequest(Guid UserId, int Rating, string Comment);
        public record CreateFeedbackResponse(Guid Id);
    }
}
