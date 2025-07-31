using InsightSage.Shared.Interfaces.DataContexts;
using InsightSage.Shared.Interfaces.DataContexts.Common;
using InsightSage.Shared.Models.Entities;

namespace InsightSage.DataContext
{
    public class UserDataContext : IUserDataContext
    {
        Task<int> IEntityDataContext<User>.AddAsync(User data)
        {
            throw new NotImplementedException();
        }

        Task<string> IEntityDataContext<User>.DeleteAsync(int id, int modifiedBy)
        {
            throw new NotImplementedException();
        }

        Task<List<User>> IEntityDataContext<User>.GetAllAsync()
        {
            throw new NotImplementedException();
        }

        Task<List<User>> IUserDataContext.GetAllByTenantIdAsync(string tenantId)
        {
            throw new NotImplementedException();
        }

        Task<User?> IUserDataContext.GetByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        Task<User> IEntityDataContext<User>.GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        Task<User?> IUserDataContext.GetByUserIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        Task<int> IEntityDataContext<User>.UpdateAsync(User data)
        {
            throw new NotImplementedException();
        }
    }
}
