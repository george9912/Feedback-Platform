using static FeedbackService.API.Features.Feedback.ListByUser.Query;

namespace FeedbackService.API.Features.Feedback.ListByUser
{
    public static class Endpoint
    {
        public static IEndpointRouteBuilder MapGetFeedbacksByUser(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/feedback/user/{userId:guid}", async (
                Guid userId,
                int page,
                int pageSize,
                Guid? viewerUserId,
                string? viewerRole,
                Handler handler,
                CancellationToken ct) =>
            {
                var request = new GetFeedbacksByUserRequest(userId, page, pageSize, viewerUserId, viewerRole);
                var result = await handler.Handle(request, ct);
                return Results.Ok(result);
            })
            .WithName("GetFeedbacksByUser")
            .Produces<IEnumerable<GetFeedbacksByUserResponse>>(StatusCodes.Status200OK);

            return app;
        }
    }
}
