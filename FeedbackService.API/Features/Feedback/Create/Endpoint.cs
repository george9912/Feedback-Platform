using Azure.Messaging.ServiceBus;
using FastEndpoints;
using FeedbackService.API.Common;
using FeedbackService.API.Common.Events;
using FeedbackService.API.Features.Clients;
using FeedbackService.API.Infrastructure;
using FluentValidation;
using System.Text.Json;
using static FeedbackService.API.Features.Feedback.Create.Command;

namespace FeedbackService.API.Features.Feedback.Create
{
    // FAST ENDPOINTS
    public sealed class CreateFeedbackEndpoint : Endpoint<CreateFeedbackRequest, CreateFeedbackResponse>
    {
        private readonly AppDbContext _db;
        private readonly IUserClient _userClient;
        private readonly ServiceBusSender _sender;
        public CreateFeedbackEndpoint(AppDbContext db, IUserClient userClient, ServiceBusSender sender)
        {
            _db = db;
            _userClient = userClient;
            _sender = sender;
        }
        public override void Configure()
        {
            Post("/api/feedback/fast");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Create feedback using FastEndpoints";
                s.Description = "Creates a new feedback entry with validation and persistence.";
            });
        }

        public override async Task HandleAsync(CreateFeedbackRequest req, CancellationToken ct)
        {
            var feedback = new Feedback(req.UserId, req.Rating, req.Comment);
            _db.Feedbacks.Add(feedback);
            await _db.SaveChangesAsync(ct);

            // publish event -> Service Bus
            var evt = new FeedbackCreatedEvent
            {
                FeedbackId = feedback.Id,
                UserId = feedback.UserId,
                Rating = feedback.Rating,
                Comment = feedback.Comment,
                CreatedAtUtc = DateTime.UtcNow
            };

            var msg = new ServiceBusMessage(JsonSerializer.Serialize(evt))
            {
                MessageId = feedback.Id.ToString(),   // idempotency-friendly
                Subject = "FeedbackCreated",
                ContentType = "application/json"
            };

            await _sender.SendMessageAsync(msg, ct);

            await this.SendCreatedAtManual(
                $"/api/feedback/{feedback.Id}",
                new CreateFeedbackResponse(feedback.Id),
                ct);
        }

    }
}
