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
    public class ChatSessionService : IChatSessionService<ChatSessionResponse<List<ChatSession>>, ChatSessionResponse<ChatSession>, ChatSession>
    {
        private readonly IChatSessionDataContext _chatSessionDataContext;
        private readonly IUserContext _userContext;
        private readonly IUserService<UserResponse<List<User>>, UserResponse<User>, User> _userService;

        public ChatSessionService(IUserContext userContext, IChatSessionDataContext chatSessionDataContext, IUserService<UserResponse<List<User>>, UserResponse<User>, User> userService)
        {
            _userContext = userContext;
            _chatSessionDataContext = chatSessionDataContext;
            _userService = userService;
        }

        async Task<AddUpdateEntityResponse> IEntityService<ChatSessionResponse<List<ChatSession>>, ChatSessionResponse<ChatSession>, ChatSession>.AddUpdateAsync(ChatSession data)
        {
            try
            {
                if (data == null)
                {
                    return new AddUpdateEntityResponse()
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = ["Chat session data is required"]
                    };
                }
                else if (data.Id > 0)
                {
                    if (string.IsNullOrWhiteSpace(data.Name))
                    {
                        return new AddUpdateEntityResponse()
                        {
                            Status = HttpStatusCode.BadRequest,
                            Errors = ["Session name is required"]
                        };
                    }
                    if (data.UniqueIdentity == Guid.Empty)
                    {
                        return new AddUpdateEntityResponse()
                        {
                            Status = HttpStatusCode.BadRequest,
                            Errors = ["Session identity is required"]
                        };
                    }
                }

                // UserId will now throw exception if not authenticated, no need to check for Guid.Empty
                data.UserId = _userContext.UserId;

                int id;
                if (data.Id == 0)
                {
                    data.UniqueIdentity = Guid.NewGuid();
                    // Add new chat session
                    id = await _chatSessionDataContext.AddAsync(data);
                }
                else
                {
                    // Update existing chat session
                    id = await _chatSessionDataContext.UpdateAsync(data);
                }

                return new AddUpdateEntityResponse
                {
                    Result = id,
                };
            }
            catch (UnauthorizedException)
            {
                return new AddUpdateEntityResponse
                {
                    Status = HttpStatusCode.Unauthorized,
                    Errors = new List<string> { "User is not authenticated or user ID is invalid. Please log in again to continue." }
                };
            }
            catch (Exception ex)
            {
                return new AddUpdateEntityResponse
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message },
                    ExceptionDetails = ex.ToString()
                };
            }
        }

        async Task<DeleteEntityResponse> IEntityService<ChatSessionResponse<List<ChatSession>>, ChatSessionResponse<ChatSession>, ChatSession>.DeleteAsync(int id, Guid userId)
        {
            try
            {
                if (id <= 0)
                {
                    return new DeleteEntityResponse
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Invalid session ID" }
                    };
                }

                var user = await _userService.GetByUserIdAsync(userId.ToString());

                var message = await _chatSessionDataContext.DeleteAsync(id, user.Result.Id);
                return new DeleteEntityResponse
                {
                    Result = message,
                };
            }
            catch (Exception ex)
            {
                return new DeleteEntityResponse
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message },
                    ExceptionDetails = ex.ToString()
                };
            }
        }

        async Task<ChatSessionResponse<List<ChatSession>>> IEntityService<ChatSessionResponse<List<ChatSession>>, ChatSessionResponse<ChatSession>, ChatSession>.GetAllAsync()
        {
            try
            {
                var sessions = await _chatSessionDataContext.GetAllAsync();
                return new ChatSessionResponse<List<ChatSession>>
                {
                    Result = sessions,
                };
            }
            catch (Exception ex)
            {
                return new ChatSessionResponse<List<ChatSession>>
                {
                    Errors = new List<string> { ex.Message },
                    Status = HttpStatusCode.InternalServerError
                };
            }
        }

        async Task<ChatSessionResponse<List<ChatSession>>> IChatSessionService<ChatSessionResponse<List<ChatSession>>, ChatSessionResponse<ChatSession>, ChatSession>.GetAllByUserIdAsync()
        {
            try
            {
                // UserId will now throw exception if not authenticated, no need to check for Guid.Empty
                var sessions = await _chatSessionDataContext.GetAllByUserIdAsync(_userContext.UserId);
                return new ChatSessionResponse<List<ChatSession>>
                {
                    Result = sessions,
                };
            }
            catch (UnauthorizedException)
            {
                return new ChatSessionResponse<List<ChatSession>>
                {
                    Errors = new List<string> { "User is not authenticated or user ID is invalid. Please log in again to continue." },
                    Status = HttpStatusCode.Unauthorized
                };
            }
            catch (Exception ex)
            {
                return new ChatSessionResponse<List<ChatSession>>
                {
                    Errors = new List<string> { ex.Message },
                    Status = HttpStatusCode.InternalServerError
                };
            }
        }

        async Task<ChatSessionResponse<ChatSession>> IEntityService<ChatSessionResponse<List<ChatSession>>, ChatSessionResponse<ChatSession>, ChatSession>.GetByIdAsync(int id)
        {
            try
            {
                var session = await _chatSessionDataContext.GetByIdAsync(id);
                return new ChatSessionResponse<ChatSession>
                {
                    Result = session,
                };
            }
            catch (Exception ex)
            {
                return new ChatSessionResponse<ChatSession>
                {
                    Errors = new List<string> { ex.Message },
                    Status = HttpStatusCode.InternalServerError
                };
            }
        }

        async Task<ChatSessionResponse<ChatSession>> IChatSessionService<ChatSessionResponse<List<ChatSession>>, ChatSessionResponse<ChatSession>, ChatSession>.GetByUniqueIdentityAsync(Guid uniqueIdentity)
        {
            try
            {
                var session = await _chatSessionDataContext.GetByUniqueIdentityAsync(uniqueIdentity);
                return new ChatSessionResponse<ChatSession>
                {
                    Result = session,
                };
            }
            catch (Exception ex)
            {
                return new ChatSessionResponse<ChatSession>
                {
                    Errors = new List<string> { ex.Message },
                    Status = HttpStatusCode.InternalServerError
                };
            }
        }

        async Task<ChatSessionResponse<List<ChatSession>>> IChatSessionService<ChatSessionResponse<List<ChatSession>>, ChatSessionResponse<ChatSession>, ChatSession>.GetSessionsByUserAndNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return new ChatSessionResponse<List<ChatSession>>
                    {
                        Errors = new List<string> { "Search name cannot be empty" },
                        Status = HttpStatusCode.BadRequest
                    };
                }

                // UserId will now throw exception if not authenticated, no need to check for Guid.Empty
                var sessions = await _chatSessionDataContext.GetSessionsByUserAndNameAsync(_userContext.UserId, name);
                return new ChatSessionResponse<List<ChatSession>>
                {
                    Result = sessions,
                };
            }
            catch (UnauthorizedException)
            {
                return new ChatSessionResponse<List<ChatSession>>
                {
                    Status = HttpStatusCode.Unauthorized,
                    Errors = new List<string> { "User is not authenticated or user ID is invalid. Please log in again to continue." }
                };
            }
            catch (Exception ex)
            {
                return new ChatSessionResponse<List<ChatSession>>
                {
                    Errors = new List<string> { ex.Message },
                    Status = HttpStatusCode.InternalServerError
                };
            }
        }

        async Task<ChatSessionResponse<ChatSession>> IChatSessionService<ChatSessionResponse<List<ChatSession>>, ChatSessionResponse<ChatSession>, ChatSession>.GetByUserIdAndExactNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return new ChatSessionResponse<ChatSession>
                    {
                        Errors = new List<string> { "Session name cannot be empty" },
                        Status = HttpStatusCode.BadRequest
                    };
                }

                // UserId will now throw exception if not authenticated, no need to check for Guid.Empty
                var session = await _chatSessionDataContext.GetByUserIdAndExactNameAsync(_userContext.UserId, name);
                return new ChatSessionResponse<ChatSession>
                {
                    Result = session,
                };
            }
            catch (UnauthorizedException)
            {
                return new ChatSessionResponse<ChatSession>
                {
                    Status = HttpStatusCode.Unauthorized,
                    Errors = new List<string> { "User is not authenticated or user ID is invalid. Please log in again to continue." }
                };
            }
            catch (Exception ex)
            {
                return new ChatSessionResponse<ChatSession>
                {
                    Errors = new List<string> { ex.Message },
                    Status = HttpStatusCode.InternalServerError
                };
            }
        }
    }
}