using InsightSage.Shared.Interfaces.DataContexts;
using InsightSage.Shared.Interfaces.Others;
using InsightSage.Shared.Interfaces.Services;
using InsightSage.Shared.Interfaces.Services.Common;
using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Responses;
using InsightSage.Shared.Models.Responses.Common;
using InsightSage.Shared.Exceptions;
using System.Net;

namespace InsightSage.Application.Services
{
    public class UserService : IUserService<UserResponse<List<User>>, UserResponse<User>, User>
    {
        private readonly IUserContext _userContext;
        private readonly IUserDataContext _userDataContext;

        public UserService(IUserContext userContext, IUserDataContext userDataContext)
        {
            _userContext = userContext;
            _userDataContext = userDataContext;
        }

        async Task<AddUpdateEntityResponse> IEntityService<UserResponse<List<User>>, UserResponse<User>, User>.AddUpdateAsync(User data)
        {
            try
            {
                int id;
                if (data.Id == 0)
                {
                    // Add new user
                    id = await _userDataContext.AddAsync(data);
                }
                else
                {
                    // Update existing user
                    id = await _userDataContext.UpdateAsync(data);
                }

                return new AddUpdateEntityResponse
                {
                    Result = id,
                };
            }
            catch (Exception ex)
            {
                return new AddUpdateEntityResponse
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        async Task<DeleteEntityResponse> IEntityService<UserResponse<List<User>>, UserResponse<User>, User>.DeleteAsync(int id, Guid userId)
        {

            try
            {
                var user = await ((IUserService<UserResponse<List<User>>, UserResponse<User>, User>)this).GetByUserIdAsync(userId.ToString());

                var message = await _userDataContext.DeleteAsync(id, user.Result.Id);
                return new DeleteEntityResponse
                {
                    Result = message,
                };
            }
            catch (Exception ex)
            {
                return new DeleteEntityResponse
                {
                    Result = $"Error: {ex.Message}",
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        async Task<UserResponse<List<User>>> IEntityService<UserResponse<List<User>>, UserResponse<User>, User>.GetAllAsync()
        {
            try
            {
                var users = await _userDataContext.GetAllAsync();
                return new UserResponse<List<User>>
                {
                    Result = users
                };
            }
            catch (Exception ex)
            {
                return new UserResponse<List<User>>
                {
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        async Task<UserResponse<List<User>>> IUserService<UserResponse<List<User>>, UserResponse<User>, User>.GetAllByTenantIdAsync(string tenantId)
        {
            try
            {
                var users = await _userDataContext.GetAllByTenantIdAsync(tenantId);
                return new UserResponse<List<User>>
                {
                    Result = users
                };
            }
            catch (Exception ex)
            {
                return new UserResponse<List<User>>
                {
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        async Task<UserResponse<User>> IUserService<UserResponse<List<User>>, UserResponse<User>, User>.GetByEmailAsync(string email)
        {
            try
            {
                var user = await _userDataContext.GetByEmailAsync(email);
                return new UserResponse<User>
                {
                    Result = user
                };
            }
            catch (Exception ex)
            {
                return new UserResponse<User>
                {
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        async Task<UserResponse<User>> IEntityService<UserResponse<List<User>>, UserResponse<User>, User>.GetByIdAsync(int id)
        {
            try
            {
                var user = await _userDataContext.GetByIdAsync(id);
                return new UserResponse<User>
                {
                    Result = user
                };
            }
            catch (Exception ex)
            {
                return new UserResponse<User>
                {
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        async Task<UserResponse<User>> IUserService<UserResponse<List<User>>, UserResponse<User>, User>.GetByUserIdAsync(string userId)
        {
            try
            {
                var user = await _userDataContext.GetByUserIdAsync(userId);
                return new UserResponse<User>
                {
                    Result = user
                };
            }
            catch (Exception ex)
            {
                return new UserResponse<User>
                {
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        async Task<UserResponse<User>> IUserService<UserResponse<List<User>>, UserResponse<User>, User>.GetCurrentUser()
        {
            try
            {
                var user = new User()
                {
                    UserId = Convert.ToString(_userContext.UserId), // UserId will throw exception if not authenticated
                    Email = _userContext.Email,
                    Name = _userContext.Name,
                    TenantId = _userContext.TenantId
                };

                return await Task.FromResult(new UserResponse<User>
                {
                    Result = user
                });
            }
            catch (UnauthorizedException)
            {
                return new UserResponse<User>
                {
                    Status = HttpStatusCode.Unauthorized,
                    Errors = new List<string> { "User is not authenticated or user ID is invalid. Please log in again to continue." }
                };
            }
            catch (Exception ex)
            {
                return new UserResponse<User>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        async Task<UserResponse<User>> IUserService<UserResponse<List<User>>, UserResponse<User>, User>.LoginAsync(User user)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    return new UserResponse<User>
                    {
                        Errors = new List<string> { "Email is required for login." }
                    };
                }

                // Check if user exists by email
                var existingUser = await _userDataContext.GetByEmailAsync(user.Email!);
                
                if (existingUser != null)
                {
                    // User exists, update last login time
                    existingUser.UpdatedAt = DateTime.UtcNow;
                    await _userDataContext.UpdateAsync(existingUser);
                    
                    return new UserResponse<User>
                    {
                        Result = existingUser
                    };
                }
                else
                {
                    // User doesn't exist, create new user
                    // Ensure Id is 0 for new entities
                    var newUser = new User
                    {
                        Id = 0, // Explicitly set to 0 for new entity
                        UserId = user.UserId,
                        Email = user.Email,
                        Name = user.Name,
                        TenantId = user.TenantId
                        // CreatedAt and UpdatedAt will be set in AddAsync
                    };
                    
                    await _userDataContext.AddAsync(newUser);
                    
                    return new UserResponse<User>
                    {
                        Result = newUser
                    };
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                // Handle unique constraint violation gracefully
                // This could happen in rare race conditions
                return new UserResponse<User>
                {
                    Errors = new List<string> { "A user with this email already exists. Please try again." }
                };
            }
            catch (Exception ex)
            {
                return new UserResponse<User>
                {
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
