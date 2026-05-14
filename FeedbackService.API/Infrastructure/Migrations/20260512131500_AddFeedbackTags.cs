using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedbackService.API.Infrastructure.Migrations
{
    public partial class AddFeedbackTags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Feedbacks",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Feedbacks");
        }
    }
}