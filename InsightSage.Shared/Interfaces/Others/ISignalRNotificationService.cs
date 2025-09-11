namespace InsightSage.Shared.Interfaces.Others
{
    public interface ISignalRNotificationService
    {
        /// <summary>
        /// Notify user that AI processing has started
        /// </summary>
        Task NotifyAIProcessingStartedAsync(Guid userId, int sessionId, int messageId);

        /// <summary>
        /// Send AI response to user
        /// </summary>
        Task SendAIResponseAsync(Guid userId, int sessionId, int messageId, string content, object? usage = null);

        /// <summary>
        /// Send streaming AI response chunk to user
        /// </summary>
        Task SendAIResponseChunkAsync(Guid userId, int sessionId, int messageId, string chunk);

        /// <summary>
        /// Notify user of AI processing completion
        /// </summary>
        Task NotifyAIProcessingCompletedAsync(Guid userId, int sessionId, int messageId);

        /// <summary>
        /// Send AI processing error to user
        /// </summary>
        Task SendAIProcessingErrorAsync(Guid userId, int sessionId, int messageId, string error);

        /// <summary>
        /// Notify user about session updates
        /// </summary>
        Task NotifySessionUpdatedAsync(Guid userId, int sessionId, string updateType);

        /// <summary>
        /// Send typing indicator
        /// </summary>
        Task SendTypingIndicatorAsync(Guid userId, int sessionId, bool isTyping);

        /// <summary>
        /// Broadcast message to all users in a session (for future collaboration features)
        /// </summary>
        Task BroadcastToSessionAsync(int sessionId, string eventName, object data);
    }
}
