using InsightSage.Shared.Interfaces.Services.Common;

namespace InsightSage.Shared.Interfaces.Services
{
    public interface IUserService<T, U, V> : IEntityService<T, U, V>
    {
        Task<U> GetCurrentUser();
        Task<U> GetByEmailAsync(string email);
        Task<U> GetByUserIdAsync(string userId);
        Task<T> GetAllByTenantIdAsync(string tenantId);
    }
}
