using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedbackService.API.Infrastructure.Migrations
{
    public partial class AddFeedbackVisibility : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Visibility",
                table: "Feedbacks",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Public");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "Feedbacks");
        }
    }
}