using FastEndpoints;
using FeedbackService.API.Common;
using FeedbackService.API.Features.Clients;
using FeedbackService.API.Infrastructure;
using FluentValidation;
using static FeedbackService.API.Features.Feedback.Create.Command;

namespace FeedbackService.API.Features.Feedback.Create
{
    // FAST ENDPOINTS
    public sealed class CreateFeedbackEndpoint : Endpoint<CreateFeedbackRequest, CreateFeedbackResponse>
    {
        private readonly AppDbContext _db;
        private readonly IUserClient _userClient;
        public CreateFeedbackEndpoint(AppDbContext db, IUserClient userClient)
        {
            _db = db;
            _userClient = userClient;
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

        //Research BFF - Backend For Frontend - alt Microserviciu ( HTTP Aggregator )
        //public override async Task HandleAsync(CreateFeedbackRequest req, CancellationToken ct)
        //{
        //    var reseponse = await _userClient.UserExistsAsync(req.UserId, ct);
        //    if (!reseponse)
        //    {
        //        HttpContext.Response.StatusCode = 404;
        //        await HttpContext.Response.WriteAsJsonAsync(new { Message = "User not found." }, ct);
        //        return;
        //    }

        //    var feedback = new Feedback(req.UserId, req.Rating, req.Comment);
        //    _db.Feedbacks.Add(feedback);
        //    await _db.SaveChangesAsync(ct);

        //    await this.SendCreatedAtManual(
        //                $"/api/feedback/{feedback.Id}",
        //                new CreateFeedbackResponse(feedback.Id),
        //                ct);
        //}

        public override async Task HandleAsync(CreateFeedbackRequest req, CancellationToken ct)
        {
            var feedback = new Feedback(req.UserId, req.Rating, req.Comment);
            _db.Feedbacks.Add(feedback);
            await _db.SaveChangesAsync(ct);

            await this.SendCreatedAtManual(
                $"/api/feedback/{feedback.Id}",
                new CreateFeedbackResponse(feedback.Id),
                ct);
        }

    }
}
