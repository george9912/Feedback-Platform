namespace FeedbackService.API.Common.Events
{
    public sealed class FeedbackCreatedEvent
    {
        public Guid EventId { get; set; }
        public Guid FeedbackId { get; set; }
        public Guid FromUserId { get; set; }
        public Guid ToUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CorrelationId { get; set; }
    }
}
