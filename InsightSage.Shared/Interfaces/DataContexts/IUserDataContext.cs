using InsightSage.Shared.Interfaces.DataContexts.Common;
using InsightSage.Shared.Models.Entities;

namespace InsightSage.Shared.Interfaces.DataContexts
{
    public interface IUserDataContext : IEntityDataContext<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUserIdAsync(string userId);
        Task<List<User>> GetAllByTenantIdAsync(string tenantId);
    }
}
