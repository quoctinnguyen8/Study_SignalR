using Microsoft.EntityFrameworkCore.Migrations;

namespace Study_SignalR.Migrations
{
    public partial class addcolumnmessageKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MessageKey",
                table: "AppUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MessageKey",
                table: "AppUsers");
        }
    }
}
