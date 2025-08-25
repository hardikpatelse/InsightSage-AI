using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsightSage.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAIModelAndChatModeRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AIModelId",
                table: "ChatSessions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChatModeId",
                table: "ChatSessions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_AIModelId",
                table: "ChatSessions",
                column: "AIModelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_ChatModeId",
                table: "ChatSessions",
                column: "ChatModeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_UserId_AIModelId",
                table: "ChatSessions",
                columns: new[] { "UserId", "AIModelId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_UserId_ChatModeId",
                table: "ChatSessions",
                columns: new[] { "UserId", "ChatModeId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ChatSessions_AIModel",
                table: "ChatSessions",
                column: "AIModelId",
                principalTable: "AIModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatSessions_ChatMode",
                table: "ChatSessions",
                column: "ChatModeId",
                principalTable: "ChatModes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatSessions_AIModel",
                table: "ChatSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatSessions_ChatMode",
                table: "ChatSessions");

            migrationBuilder.DropIndex(
                name: "IX_ChatSessions_AIModelId",
                table: "ChatSessions");

            migrationBuilder.DropIndex(
                name: "IX_ChatSessions_ChatModeId",
                table: "ChatSessions");

            migrationBuilder.DropIndex(
                name: "IX_ChatSessions_UserId_AIModelId",
                table: "ChatSessions");

            migrationBuilder.DropIndex(
                name: "IX_ChatSessions_UserId_ChatModeId",
                table: "ChatSessions");

            migrationBuilder.DropColumn(
                name: "AIModelId",
                table: "ChatSessions");

            migrationBuilder.DropColumn(
                name: "ChatModeId",
                table: "ChatSessions");
        }
    }
}
