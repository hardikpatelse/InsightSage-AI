using InsightSage.Shared.Models.Entities;

namespace InsightSage.Shared.Models.Requests
{
    /// <summary>
    /// Request model for creating a chat message with AI model and chat mode configuration
    /// </summary>
    public class MessageWithConfigRequest
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
    }
}
