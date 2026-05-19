using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedbackService.API.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
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
                END

                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_UserNotifications_EventId'
                      AND object_id = OBJECT_ID('dbo.UserNotifications'))
                BEGIN
                    CREATE UNIQUE INDEX [IX_UserNotifications_EventId]
                        ON [dbo].[UserNotifications]([EventId]);
                END

                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_UserNotifications_RecipientUserId_CreatedAtUtc'
                      AND object_id = OBJECT_ID('dbo.UserNotifications'))
                BEGIN
                    CREATE INDEX [IX_UserNotifications_RecipientUserId_CreatedAtUtc]
                        ON [dbo].[UserNotifications]([RecipientUserId], [CreatedAtUtc]);
                END

                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_UserNotifications_RecipientUserId_IsRead'
                      AND object_id = OBJECT_ID('dbo.UserNotifications'))
                BEGIN
                    CREATE INDEX [IX_UserNotifications_RecipientUserId_IsRead]
                        ON [dbo].[UserNotifications]([RecipientUserId], [IsRead]);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID('dbo.UserNotifications', 'U') IS NOT NULL
                BEGIN
                    DROP TABLE [dbo].[UserNotifications];
                END
                """);
        }
    }
}
