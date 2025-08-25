using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsightSage.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAIPlatformTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIPlatforms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ApiBaseUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DocumentationUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RequiresApiKey = table.Column<bool>(type: "bit", nullable: false),
                    AuthHeaderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "Authorization"),
                    AuthHeaderFormat = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, defaultValue: "Bearer {api_key}"),
                    RateLimitPerMinute = table.Column<int>(type: "int", nullable: false),
                    DefaultTimeoutSeconds = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ConfigurationJson = table.Column<string>(type: "ntext", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIPlatforms", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIPlatforms_DisplayOrder",
                table: "AIPlatforms",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_AIPlatforms_IsActive",
                table: "AIPlatforms",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AIPlatforms_Name_Unique",
                table: "AIPlatforms",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIPlatforms_Slug_Unique",
                table: "AIPlatforms",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIPlatforms");
        }
    }
}
