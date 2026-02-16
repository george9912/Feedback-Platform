using FluentValidation;
using static FeedbackService.API.Features.Feedback.Update.Command;

namespace FeedbackService.API.Features.Feedback.Update
{
    public static class Endpoint
    {
        public static IEndpointRouteBuilder MapUpdateFeedback(this IEndpointRouteBuilder app)
        {
            app.MapPut("/api/feedback/{id:guid}", async (
                Guid id,
                UpdateFeedbackRequest request,
                Handler handler,
                IValidator<UpdateFeedbackRequest> validator,
                CancellationToken ct) =>
            {
                var validation = await validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                    return Results.ValidationProblem(validation.ToDictionary());

                var ok = await handler.Handle(id, request, ct);
                return ok ? Results.NoContent() : Results.NotFound();
            })
            .WithName("UpdateFeedback")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

            return app;
        }
    }
}
