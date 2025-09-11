using InsightSage.Shared.Interfaces.Services.Common;
using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Responses.Common;

namespace InsightSage.Shared.Interfaces.Services
{
    public interface IChatMessageService<T, U, V> : IEntityService<T, U, V>
    {
        Task<T> GetBySessionIdAsync(int sessionId);
        Task<AddUpdateEntityResponse> CreateMessageWithConfigurationAsync(ChatMessage data, int? aiModelId = null, int? chatModeId = null);
    }
}