using InsightSage.Shared.Interfaces.Services.Common;

namespace InsightSage.Shared.Interfaces.Services
{
    public interface IChatMessageService<T, U, V> : IEntityService<T, U, V>
    {
        Task<T> GetBySessionIdAsync(int sessionId);
    }
}