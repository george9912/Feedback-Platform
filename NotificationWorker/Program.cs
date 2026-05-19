using Microsoft.EntityFrameworkCore;
using NotificationWorker.Data;
using NotificationWorker.Messaging;
using NotificationWorker.Processing;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<NotificationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("NotificationConnection")
        ?? throw new InvalidOperationException("Missing ConnectionStrings:NotificationConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure());
});

builder.Services.AddScoped<IFeedbackCreatedProcessor, FeedbackCreatedProcessor>();
builder.Services.AddHostedService<RabbitMqFeedbackCreatedConsumer>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await EnsureNotificationTablesAsync(db);
}

host.Run();

static async Task EnsureNotificationTablesAsync(NotificationDbContext db)
{
    const string createProcessedNotificationsTableSql = """
    IF OBJECT_ID('dbo.ProcessedNotifications', 'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[ProcessedNotifications] (
            [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
            [EventId] UNIQUEIDENTIFIER NOT NULL,
            [FeedbackId] UNIQUEIDENTIFIER NOT NULL,
            [FromUserId] UNIQUEIDENTIFIER NOT NULL,
            [ToUserId] UNIQUEIDENTIFIER NOT NULL,
            [FeedbackCreatedAt] DATETIME2 NOT NULL,
            [CorrelationId] NVARCHAR(128) NULL,
            [ProcessedAtUtc] DATETIME2 NOT NULL
        );

        CREATE UNIQUE INDEX [IX_ProcessedNotifications_EventId]
            ON [dbo].[ProcessedNotifications]([EventId]);

        CREATE UNIQUE INDEX [IX_ProcessedNotifications_FeedbackId_ToUserId]
            ON [dbo].[ProcessedNotifications]([FeedbackId], [ToUserId]);
    END
    """;

    const string createUserNotificationsTableSql = """
    IF OBJECT_ID('dbo.UserNotifications', 'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[UserNotifications] (
            [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
            [EventId] UNIQUEIDENTIFIER NOT NULL,
            [RecipientUserId] UNIQUEIDENTIFIER NOT NULL,
            [ActorUserId] UNIQUEIDENTIFIER NOT NULL,
            [FeedbackId] UNIQUEIDENTIFIER NOT NULL,
            [Message] NVARCHAR(500) NOT NULL,
            [IsRead] BIT NOT NULL,
            [CreatedAtUtc] DATETIME2 NOT NULL,
            [ReadAtUtc] DATETIME2 NULL
        );

        CREATE UNIQUE INDEX [IX_UserNotifications_EventId]
            ON [dbo].[UserNotifications]([EventId]);

        CREATE INDEX [IX_UserNotifications_RecipientUserId_CreatedAtUtc]
            ON [dbo].[UserNotifications]([RecipientUserId], [CreatedAtUtc]);

        CREATE INDEX [IX_UserNotifications_RecipientUserId_IsRead]
            ON [dbo].[UserNotifications]([RecipientUserId], [IsRead]);
    END
    """;

    const string backfillUserNotificationsSql = """
    INSERT INTO [dbo].[UserNotifications]
        ([Id], [EventId], [RecipientUserId], [ActorUserId], [FeedbackId], [Message], [IsRead], [CreatedAtUtc], [ReadAtUtc])
    SELECT
        NEWID(),
        p.[EventId],
        p.[ToUserId],
        p.[FromUserId],
        p.[FeedbackId],
        N'You received new feedback.',
        CAST(0 AS bit),
        p.[ProcessedAtUtc],
        NULL
    FROM [dbo].[ProcessedNotifications] p
    LEFT JOIN [dbo].[UserNotifications] u ON u.[EventId] = p.[EventId]
    WHERE u.[EventId] IS NULL;
    """;

    await db.Database.ExecuteSqlRawAsync(createProcessedNotificationsTableSql);
    await db.Database.ExecuteSqlRawAsync(createUserNotificationsTableSql);
    await db.Database.ExecuteSqlRawAsync(backfillUserNotificationsSql);
}
