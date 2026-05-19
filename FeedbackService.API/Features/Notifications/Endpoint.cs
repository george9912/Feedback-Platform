using FeedbackService.API.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FeedbackService.API.Features.Notifications;

public static class Endpoint
{
    public static IEndpointRouteBuilder MapNotificationRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/notifications/user/{userId:guid}", ListByUser);
        app.MapPost("/api/notifications/{notificationId:guid}/read", MarkRead);

        return app;
    }

    private static async Task<IResult> ListByUser(
        Guid userId,
        int page,
        int pageSize,
        AppDbContext db,
        CancellationToken ct)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize is < 1 or > 50 ? 20 : pageSize;

        var baseQuery = db.UserNotifications
            .AsNoTracking()
            .Where(x => x.RecipientUserId == userId);

        var totalCount = await baseQuery.CountAsync(ct);
        var unreadCount = await baseQuery.CountAsync(x => !x.IsRead, ct);

        var items = await baseQuery
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(x => new NotificationListItem(
                x.Id,
                x.EventId,
                x.RecipientUserId,
                x.ActorUserId,
                x.FeedbackId,
                x.Message,
                x.IsRead,
                x.CreatedAtUtc,
                x.ReadAtUtc))
            .ToListAsync(ct);

        return Results.Ok(new NotificationListResponse(
            items,
            totalCount,
            unreadCount,
            normalizedPage,
            normalizedPageSize));
    }

    private static async Task<IResult> MarkRead(
        Guid notificationId,
        MarkNotificationReadRequest request,
        AppDbContext db,
        CancellationToken ct)
    {
        var notification = await db.UserNotifications
            .FirstOrDefaultAsync(x => x.Id == notificationId, ct);

        if (notification is null)
        {
            return Results.NotFound();
        }

        if (notification.RecipientUserId != request.UserId)
        {
            return Results.Forbid();
        }

        notification.MarkRead(DateTime.UtcNow);
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    public sealed record NotificationListItem(
        Guid Id,
        Guid EventId,
        Guid RecipientUserId,
        Guid ActorUserId,
        Guid FeedbackId,
        string Message,
        bool IsRead,
        DateTime CreatedAtUtc,
        DateTime? ReadAtUtc);

    public sealed record NotificationListResponse(
        IReadOnlyList<NotificationListItem> Items,
        int TotalCount,
        int UnreadCount,
        int Page,
        int PageSize);

    public sealed record MarkNotificationReadRequest(Guid UserId);
}
