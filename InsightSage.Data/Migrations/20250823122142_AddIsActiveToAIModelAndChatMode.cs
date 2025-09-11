using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsightSage.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToAIModelAndChatMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ChatModes",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AIModels",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatModes_IsActive",
                table: "ChatModes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AIModels_IsActive",
                table: "AIModels",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatModes_IsActive",
                table: "ChatModes");

            migrationBuilder.DropIndex(
                name: "IX_AIModels_IsActive",
                table: "AIModels");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ChatModes");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AIModels");
        }
    }
}
