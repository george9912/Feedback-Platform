namespace FeedbackService.API.Features.Feedback.Delete
{
    public static class Endpoint
    {
        public static IEndpointRouteBuilder MapDeleteFeedback(this IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/feedback/{id:guid}", async (
                Guid id,
                Handler handler,
                CancellationToken ct) =>
            {
                var deleted = await handler.Handle(id, ct);
                return deleted ? Results.NoContent() : Results.NotFound();
            })
            .WithName("DeleteFeedback")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            return app;
        }
    }
}
