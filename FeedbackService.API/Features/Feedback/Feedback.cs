namespace FeedbackService.API.Features.Feedback
{
    public class Feedback
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public int Rating { get; private set; }
        public string Comment { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }

        private Feedback() { } 

        public Feedback(Guid userId, int rating, string comment)
        {
            if (rating is < 1 or > 5)
                throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be 1..5");

            Id = Guid.NewGuid();
            UserId = userId;
            Rating = rating;
            Comment = comment.Trim();
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(int rating, string comment)
        {
            if (rating is < 1 or > 5)
                throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5");

            Rating = rating;
            Comment = comment?.Trim() ?? string.Empty;
        }
    }
}
