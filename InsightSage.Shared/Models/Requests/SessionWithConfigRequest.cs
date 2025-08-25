namespace InsightSage.Shared.Models.Requests
{
    /// <summary>
    /// Request model for creating a chat session with AI model and chat mode configuration
    /// </summary>
    public class SessionWithConfigRequest
    {
        /// <summary>
        /// Name of the chat session
        /// </summary>
        public string Name { get; set; } = string.Empty;

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
