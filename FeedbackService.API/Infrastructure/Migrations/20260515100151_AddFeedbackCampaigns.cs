using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedbackService.API.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedbackCampaigns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CampaignId",
                table: "Feedbacks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmittedByUserId",
                table: "Feedbacks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CampaignFinalReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalParticipants = table.Column<int>(type: "int", nullable: false),
                    CompletedParticipants = table.Column<int>(type: "int", nullable: false),
                    InProgressParticipants = table.Column<int>(type: "int", nullable: false),
                    NotStartedParticipants = table.Column<int>(type: "int", nullable: false),
                    TotalSubmissions = table.Column<int>(type: "int", nullable: false),
                    GeneratedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignFinalReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    StartDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    MinimumRequiredSubmissions = table.Column<int>(type: "int", nullable: false),
                    IsAnonymous = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByAdminId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignAudienceRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AudienceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RuleValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignAudienceRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignAudienceRules_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequiredSubmissions = table.Column<int>(type: "int", nullable: false),
                    SubmittedSubmissions = table.Column<int>(type: "int", nullable: false),
                    InvitedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSubmissionAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignParticipants_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_CampaignId",
                table: "Feedbacks",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_CampaignId_SubmittedByUserId",
                table: "Feedbacks",
                columns: new[] { "CampaignId", "SubmittedByUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignAudienceRules_CampaignId_AudienceType",
                table: "CampaignAudienceRules",
                columns: new[] { "CampaignId", "AudienceType" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignFinalReports_CampaignId",
                table: "CampaignFinalReports",
                column: "CampaignId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignParticipants_CampaignId_UserId",
                table: "CampaignParticipants",
                columns: new[] { "CampaignId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignParticipants_UserId",
                table: "CampaignParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_Status",
                table: "Campaigns",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignAudienceRules");

            migrationBuilder.DropTable(
                name: "CampaignFinalReports");

            migrationBuilder.DropTable(
                name: "CampaignParticipants");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_CampaignId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_CampaignId_SubmittedByUserId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                table: "Feedbacks");
        }
    }
}
