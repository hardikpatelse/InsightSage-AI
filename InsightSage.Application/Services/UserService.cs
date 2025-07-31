using InsightSage.Shared.Interfaces.Others;
using InsightSage.Shared.Interfaces.Services;
using InsightSage.Shared.Interfaces.Services.Common;
using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Responses;
using InsightSage.Shared.Models.Responses.Common;

namespace InsightSage.Application.Services
{
    public class UserService : IUserService<UserResponse<List<User>>, UserResponse<User>, User>
    {
        private readonly IUserContext _userContext;
        public UserService(IUserContext userContext)
        {
            _userContext = userContext;
        }
        Task<AddUpdateEntityResponse> IEntityService<UserResponse<List<User>>, UserResponse<User>, User>.AddUpdateAsync(User data)
        {
            throw new NotImplementedException();
        }

        Task<DeleteEntityResponse> IEntityService<UserResponse<List<User>>, UserResponse<User>, User>.DeleteAsync(int id, int modifiedBy)
        {
            throw new NotImplementedException();
        }

        Task<UserResponse<List<User>>> IEntityService<UserResponse<List<User>>, UserResponse<User>, User>.GetAllAsync()
        {
            throw new NotImplementedException();
        }

        Task<UserResponse<List<User>>> IUserService<UserResponse<List<User>>, UserResponse<User>, User>.GetAllByTenantIdAsync(string tenantId)
        {
            throw new NotImplementedException();
        }

        Task<UserResponse<User>> IUserService<UserResponse<List<User>>, UserResponse<User>, User>.GetByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        Task<UserResponse<User>> IEntityService<UserResponse<List<User>>, UserResponse<User>, User>.GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        Task<UserResponse<User>> IUserService<UserResponse<List<User>>, UserResponse<User>, User>.GetByUserIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        async Task<UserResponse<User>> IUserService<UserResponse<List<User>>, UserResponse<User>, User>.GetCurrentUser()
        {
            var user = new User()
            {
                UserId = _userContext.UserId,
                Email = _userContext.Email,
                Name = _userContext.Name,
                TenantId = _userContext.TenantId
            };

            return await Task.FromResult(new UserResponse<User>
            {
                Result = user
            });
        }
    }
}
