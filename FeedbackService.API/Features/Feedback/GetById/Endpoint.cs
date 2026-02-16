using static FeedbackService.API.Features.Feedback.GetById.Query;

namespace FeedbackService.API.Features.Feedback.GetById
{
    public static class Endpoint
    {
        public static IEndpointRouteBuilder MapGetFeedbackById(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/feedback/{id:guid}", async (
                Guid id,
                Handler handler,
                CancellationToken ct) =>
            {
                var feedback = await handler.Handle(id, ct);
                return feedback is null ? Results.NotFound() : Results.Ok(feedback);
            })
            .WithName("GetFeedbackById")
            .Produces<GetFeedbackByIdResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            return app;
        }
    }
}
