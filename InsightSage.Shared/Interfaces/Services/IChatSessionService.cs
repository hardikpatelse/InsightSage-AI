using InsightSage.Shared.Interfaces.Services.Common;

namespace InsightSage.Shared.Interfaces.Services
{
    public interface IChatSessionService<T, U, V> : IEntityService<T, U, V>
    {
        Task<T> GetAllByUserIdAsync();
        Task<U> GetByUniqueIdentityAsync(Guid uniqueIdentity);
        Task<T> GetSessionsByUserAndNameAsync(string name);
        Task<U> GetByUserIdAndExactNameAsync(string name);
    }
}