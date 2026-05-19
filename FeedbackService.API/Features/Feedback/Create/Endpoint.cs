using FastEndpoints;
using FeedbackService.API.Common;
using FluentValidation;
using static FeedbackService.API.Features.Feedback.Create.Command;

namespace FeedbackService.API.Features.Feedback.Create
{
    // FAST ENDPOINTS
    public sealed class CreateFeedbackEndpoint : Endpoint<CreateFeedbackRequest, CreateFeedbackResponse>
    {
        private readonly Handler _handler;

        public CreateFeedbackEndpoint(Handler handler)
        {
            _handler = handler;
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
            var result = await _handler.Handle(req, ct);

            if (result.IsFailure || result.Value is null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await HttpContext.Response.WriteAsJsonAsync(
                    new { message = result.Error ?? "Failed to create feedback." },
                    cancellationToken: ct);

                return;
            }

            await this.SendCreatedAtManual(
                $"/api/feedback/{result.Value.Id}",
                result.Value,
                ct);
        }

    }
}
