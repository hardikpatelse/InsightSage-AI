using InsightSage.Shared.Models.Entities;

namespace InsightSage.Shared.Models.Requests
{
    /// <summary>
    /// Request model for creating a chat message with AI processing
    /// </summary>
    public class MessageWithAIRequest
    {
        /// <summary>
        /// The chat message to create
        /// </summary>
        public ChatMessage Message { get; set; } = new();

        /// <summary>
        /// ID of the AI model to use (optional)
        /// </summary>
        public int? AIModelId { get; set; }

        /// <summary>
        /// ID of the chat mode to use (optional)
        /// </summary>
        public int? ChatModeId { get; set; }

        /// <summary>
        /// Whether to process this message with AI
        /// </summary>
        public bool ProcessWithAI { get; set; } = true;

        /// <summary>
        /// AI Platform slug (e.g., "openai", "anthropic")
        /// </summary>
        public string? PlatformSlug { get; set; } = "openai";

        /// <summary>
        /// AI Model name (e.g., "gpt-3.5-turbo", "gpt-4")
        /// </summary>
        public string? Model { get; set; } = "gpt-3.5-turbo";

        /// <summary>
        /// Maximum tokens for AI response
        /// </summary>
        public int? MaxTokens { get; set; } = 1000;

        /// <summary>
        /// Temperature for AI response (0.0 to 1.0)
        /// </summary>
        public double? Temperature { get; set; } = 0.7;
    }
}