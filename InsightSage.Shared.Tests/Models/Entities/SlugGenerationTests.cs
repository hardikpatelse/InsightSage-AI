using InsightSage.Shared.Models.Entities;
using Xunit;

namespace InsightSage.Shared.Tests.Models.Entities
{
    /// <summary>
    /// Unit tests for AIModel and ChatMode slug generation
    /// </summary>
    public class SlugGenerationTests
    {
        [Fact]
        public void AIModel_Name_SetsSlugCorrectly()
        {
            // Arrange & Act
            var model = new AIModel { Name = "GPT-4 Turbo Model" };
            
            // Assert
            Assert.Equal("gpt-4_turbo_model", model.Slug);
        }

        [Fact]
        public void AIModel_NameWithSpaces_ReplacesSpacesWithUnderscores()
        {
            // Arrange & Act
            var model = new AIModel { Name = "Claude 3 Haiku" };
            
            // Assert
            Assert.Equal("claude_3_haiku", model.Slug);
        }

        [Fact]
        public void AIModel_NameWithMixedCase_ConvertsToLowercase()
        {
            // Arrange & Act
            var model = new AIModel { Name = "OpenAI GPT Model" };
            
            // Assert
            Assert.Equal("openai_gpt_model", model.Slug);
        }

        [Fact]
        public void AIModel_EmptyName_SetsEmptySlug()
        {
            // Arrange & Act
            var model = new AIModel { Name = "" };
            
            // Assert
            Assert.Equal("", model.Slug);
        }

        [Fact]
        public void AIModel_WhitespaceName_SetsEmptySlug()
        {
            // Arrange & Act
            var model = new AIModel { Name = "   " };
            
            // Assert
            Assert.Equal("", model.Slug);
        }

        [Fact]
        public void AIModel_NameWithTabs_ReplacesTabsWithUnderscores()
        {
            // Arrange & Act
            var model = new AIModel { Name = "Model\tWith\tTabs" };
            
            // Assert
            Assert.Equal("model_with_tabs", model.Slug);
        }

        [Fact]
        public void ChatMode_Name_SetsSlugCorrectly()
        {
            // Arrange & Act
            var chatMode = new ChatMode { Name = "Creative Writing Mode" };
            
            // Assert
            Assert.Equal("creative_writing_mode", chatMode.Slug);
        }

        [Fact]
        public void ChatMode_NameWithSpaces_ReplacesSpacesWithUnderscores()
        {
            // Arrange & Act
            var chatMode = new ChatMode { Name = "Code Assistant" };
            
            // Assert
            Assert.Equal("code_assistant", chatMode.Slug);
        }

        [Fact]
        public void ChatMode_NameWithMixedCase_ConvertsToLowercase()
        {
            // Arrange & Act
            var chatMode = new ChatMode { Name = "Technical SUPPORT Mode" };
            
            // Assert
            Assert.Equal("technical_support_mode", chatMode.Slug);
        }

        [Fact]
        public void ChatMode_EmptyName_SetsEmptySlug()
        {
            // Arrange & Act
            var chatMode = new ChatMode { Name = "" };
            
            // Assert
            Assert.Equal("", chatMode.Slug);
        }

        [Fact]
        public void ChatMode_WhitespaceName_SetsEmptySlug()
        {
            // Arrange & Act
            var chatMode = new ChatMode { Name = "   " };
            
            // Assert
            Assert.Equal("", chatMode.Slug);
        }

        [Fact]
        public void ChatMode_NameUpdateChangesSlug()
        {
            // Arrange
            var chatMode = new ChatMode { Name = "Initial Name" };
            Assert.Equal("initial_name", chatMode.Slug);
            
            // Act
            chatMode.Name = "Updated Name";
            
            // Assert
            Assert.Equal("updated_name", chatMode.Slug);
        }

        [Fact]
        public void AIModel_NameUpdateChangesSlug()
        {
            // Arrange
            var model = new AIModel { Name = "Initial Model" };
            Assert.Equal("initial_model", model.Slug);
            
            // Act
            model.Name = "Updated Model";
            
            // Assert
            Assert.Equal("updated_model", model.Slug);
        }
    }
}