using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsightSage.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedAIModelAndChatModeData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed AIModel data
            migrationBuilder.InsertData(
                table: "AIModels",
                columns: new[] { "Name", "Description", "Slug" },
                values: new object[,]
                {
                    { "GPT-4 Turbo", "Latest GPT-4 model with improved performance and lower costs", "gpt-4_turbo" },
                    { "GPT-4", "Most capable GPT-4 model, great for complex tasks requiring creativity and reasoning", "gpt-4" },
                    { "GPT-3.5 Turbo", "Fast, cost-effective model for simple tasks", "gpt-3.5_turbo" },
                    { "Claude 3 Opus", "Anthropic's most powerful model for complex reasoning tasks", "claude_3_opus" },
                    { "Claude 3 Sonnet", "Anthropic's balanced model for general-purpose tasks", "claude_3_sonnet" },
                    { "Claude 3 Haiku", "Anthropic's fastest model for simple tasks", "claude_3_haiku" },
                    { "Gemini Pro", "Google's advanced AI model for complex reasoning and coding", "gemini_pro" },
                    { "Gemini Flash", "Google's fast AI model optimized for speed and efficiency", "gemini_flash" }
                });

            // Seed ChatMode data
            migrationBuilder.InsertData(
                table: "ChatModes",
                columns: new[] { "Name", "Description", "Slug", "IconName" },
                values: new object[,]
                {
                    { "General Chat", "General conversation mode for everyday interactions", "general_chat", "pi pi-comments" },
                    { "Creative Writing", "Mode optimized for creative writing, storytelling, and content creation", "creative_writing", "pi pi-pencil" },
                    { "Code Assistant", "Programming and development assistance with code generation and debugging", "code_assistant", "pi pi-code" },
                    { "Research Helper", "Research assistance with fact-checking and information gathering", "research_helper", "pi pi-search" },
                    { "Technical Support", "Technical troubleshooting and problem-solving assistance", "technical_support", "pi pi-wrench" },
                    { "Educational Tutor", "Learning and teaching assistance across various subjects", "educational_tutor", "pi pi-book" },
                    { "Business Advisor", "Business strategy, analysis, and professional guidance", "business_advisor", "pi pi-briefcase" },
                    { "Language Translator", "Multi-language translation and communication assistance", "language_translator", "pi pi-globe" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seed data
            migrationBuilder.DeleteData(
                table: "AIModels",
                keyColumn: "Slug",
                keyValues: new object[]
                {
                    "gpt-4_turbo",
                    "gpt-4",
                    "gpt-3.5_turbo",
                    "claude_3_opus",
                    "claude_3_sonnet",
                    "claude_3_haiku",
                    "gemini_pro",
                    "gemini_flash"
                });

            migrationBuilder.DeleteData(
                table: "ChatModes",
                keyColumn: "Slug",
                keyValues: new object[]
                {
                    "general_chat",
                    "creative_writing",
                    "code_assistant",
                    "research_helper",
                    "technical_support",
                    "educational_tutor",
                    "business_advisor",
                    "language_translator"
                });
        }
    }
}
