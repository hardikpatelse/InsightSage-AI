using InsightSage.Shared.Interfaces.DataContexts.Common;
using InsightSage.Shared.Models.Entities;

namespace InsightSage.Shared.Interfaces.DataContexts
{
    public interface IChatMessageDataContext : IEntityDataContext<ChatMessage>
    {
        Task<List<ChatMessage>> GetBySessionIdAsync(int sessionId);
    }
}