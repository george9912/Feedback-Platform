namespace FeedbackService.API.Features.Feedback.Update
{
    public class Command
    {
        public sealed record UpdateFeedbackRequest(int Rating, string Comment);
    }
}
