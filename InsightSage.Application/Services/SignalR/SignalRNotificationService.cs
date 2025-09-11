using InsightSage.Application.Services.SignalR.Hubs;
using InsightSage.Shared.Interfaces.Others;
using Microsoft.AspNetCore.SignalR;

namespace InsightSage.Application.Services.SignalR
{
    public class SignalRNotificationService : ISignalRNotificationService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public SignalRNotificationService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyAIProcessingStartedAsync(Guid userId, int sessionId, int messageId)
        {
            await _hubContext.Clients.Group($"user-{userId}")
                .SendAsync("AIProcessingStarted", new
                {
                    SessionId = sessionId,
                    MessageId = messageId,
                    Status = "processing",
                    Timestamp = DateTime.UtcNow
                });
        }

        public async Task SendAIResponseAsync(Guid userId, int sessionId, int messageId, string content, object? usage = null)
        {
            await _hubContext.Clients.Group($"user-{userId}")
                .SendAsync("AIResponseReceived", new
                {
                    SessionId = sessionId,
                    MessageId = messageId,
                    Role = "assistant",
                    Content = content,
                    Timestamp = DateTime.UtcNow,
                    Usage = usage
                });
        }

        public async Task SendAIResponseChunkAsync(Guid userId, int sessionId, int messageId, string chunk)
        {
            await _hubContext.Clients.Group($"user-{userId}")
                .SendAsync("AIResponseChunk", new
                {
                    SessionId = sessionId,
                    MessageId = messageId,
                    Chunk = chunk,
                    Timestamp = DateTime.UtcNow
                });
        }

        public async Task NotifyAIProcessingCompletedAsync(Guid userId, int sessionId, int messageId)
        {
            await _hubContext.Clients.Group($"user-{userId}")
                .SendAsync("AIProcessingCompleted", new
                {
                    SessionId = sessionId,
                    MessageId = messageId,
                    Status = "completed",
                    Timestamp = DateTime.UtcNow
                });
        }

        public async Task SendAIProcessingErrorAsync(Guid userId, int sessionId, int messageId, string error)
        {
            await _hubContext.Clients.Group($"user-{userId}")
                .SendAsync("AIProcessingError", new
                {
                    SessionId = sessionId,
                    MessageId = messageId,
                    Error = error,
                    Timestamp = DateTime.UtcNow
                });
        }

        public async Task NotifySessionUpdatedAsync(Guid userId, int sessionId, string updateType)
        {
            await _hubContext.Clients.Group($"user-{userId}")
                .SendAsync("SessionUpdated", new
                {
                    SessionId = sessionId,
                    UpdateType = updateType,
                    Timestamp = DateTime.UtcNow
                });
        }

        public async Task SendTypingIndicatorAsync(Guid userId, int sessionId, bool isTyping)
        {
            await _hubContext.Clients.Group($"user-{userId}")
                .SendAsync("TypingIndicator", new
                {
                    SessionId = sessionId,
                    IsTyping = isTyping,
                    Timestamp = DateTime.UtcNow
                });
        }

        public async Task BroadcastToSessionAsync(int sessionId, string eventName, object data)
        {
            await _hubContext.Clients.Group($"session-{sessionId}")
                .SendAsync(eventName, data);
        }
    }
}
