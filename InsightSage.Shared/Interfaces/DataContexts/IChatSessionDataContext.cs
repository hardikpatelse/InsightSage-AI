using InsightSage.Shared.Interfaces.DataContexts.Common;
using InsightSage.Shared.Models.Entities;

namespace InsightSage.Shared.Interfaces.DataContexts
{
    public interface IChatSessionDataContext : IEntityDataContext<ChatSession>
    {
        Task<List<ChatSession>> GetAllByUserIdAsync(Guid userId);
        Task<ChatSession?> GetByUniqueIdentityAsync(Guid uniqueIdentity);
        Task<List<ChatSession>> GetSessionsByUserAndNameAsync(Guid userId, string name);
        Task<ChatSession?> GetByUserIdAndExactNameAsync(Guid userId, string name);
    }
}