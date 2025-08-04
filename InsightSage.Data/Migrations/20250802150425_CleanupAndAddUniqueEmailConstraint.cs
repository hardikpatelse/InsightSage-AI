using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsightSage.Data.Migrations
{
    /// <inheritdoc />
    public partial class CleanupAndAddUniqueEmailConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, drop the existing non-unique index
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            // Clean up duplicate email entries - keep the first (oldest) record for each email
            migrationBuilder.Sql(@"
                WITH DuplicateEmails AS (
                    SELECT Id, Email, 
                           ROW_NUMBER() OVER (PARTITION BY Email ORDER BY CreatedAt ASC) as RowNum
                    FROM Users 
                    WHERE Email IS NOT NULL
                )
                DELETE FROM Users 
                WHERE Id IN (
                    SELECT Id FROM DuplicateEmails WHERE RowNum > 1
                );
            ");

            // Now create the unique index
            migrationBuilder.CreateIndex(
                name: "IX_Users_Email_Unique",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email_Unique",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");
        }
    }
}
