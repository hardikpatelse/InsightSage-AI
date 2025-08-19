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
    public class ChatMessageService : IChatMessageService<ChatMessageResponse<List<ChatMessage>>, ChatMessageResponse<ChatMessage>, ChatMessage>
    {
        private readonly IChatMessageDataContext _chatMessageDataContext;
        private readonly IUserContext _userContext;
        private readonly IChatSessionService<ChatSessionResponse<List<ChatSession>>, ChatSessionResponse<ChatSession>, ChatSession> _chatSessionService;
        private readonly IUserService<UserResponse<List<User>>, UserResponse<User>, User> _userService;

        public ChatMessageService(IUserContext userContext, 
            IChatMessageDataContext chatMessageDataContext,
            IChatSessionService<ChatSessionResponse<List<ChatSession>>, ChatSessionResponse<ChatSession>, ChatSession> chatSessionService,
            IUserService<UserResponse<List<User>>, UserResponse<User>, User> userService)
        {
            _userContext = userContext;
            _chatMessageDataContext = chatMessageDataContext;
            _chatSessionService = chatSessionService;
            _userService = userService;
        }

        async Task<AddUpdateEntityResponse> IEntityService<ChatMessageResponse<List<ChatMessage>>, ChatMessageResponse<ChatMessage>, ChatMessage>.AddUpdateAsync(ChatMessage data)
        {
            try
            {
                if (data == null)
                {
                    return new AddUpdateEntityResponse
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Chat message data is required" }
                    };
                }

                if (string.IsNullOrWhiteSpace(data.Role))
                {
                    return new AddUpdateEntityResponse
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Message role is required" }
                    };
                }

                if (string.IsNullOrWhiteSpace(data.Content))
                {
                    return new AddUpdateEntityResponse
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Message content is required" }
                    };
                }

                // Handle default session logic when ChatSessionId is 0
                if (data.ChatSessionId <= 0)
                {
                    const string defaultSessionName = "Default Session";
                    
                    // Check if Default Session exists
                    var defaultSessionResponse = await _chatSessionService.GetByUserIdAndExactNameAsync(defaultSessionName);
                    
                    if (defaultSessionResponse.Result != null)
                    {
                        // Use existing default session
                        data.ChatSessionId = defaultSessionResponse.Result.Id;
                    }
                    else
                    {
                        // Create new default session
                        var newDefaultSession = new ChatSession
                        {
                            Name = defaultSessionName,
                            UserId = _userContext.UserId, // UserId will throw exception if not authenticated
                            UniqueIdentity = Guid.NewGuid()
                        };

                        var createSessionResponse = await _chatSessionService.AddUpdateAsync(newDefaultSession);
                        
                        if (createSessionResponse.Result > 0)
                        {
                            data.ChatSessionId = createSessionResponse.Result;
                        }
                        else
                        {
                            return new AddUpdateEntityResponse
                            {
                                Status = HttpStatusCode.InternalServerError,
                                Errors = new List<string> { "Failed to create default session" }
                            };
                        }
                    }
                } 
                else if (data.ChatSessionId <= 0)
                {
                    return new AddUpdateEntityResponse
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Valid chat session is required" }
                    };
                }

                int id;
                if (data.Id == 0)
                {
                    // Add new chat message
                    id = await _chatMessageDataContext.AddAsync(data);
                }
                else
                {
                    // Update existing chat message
                    id = await _chatMessageDataContext.UpdateAsync(data);
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
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        async Task<ChatMessageResponse<List<ChatMessage>>> IEntityService<ChatMessageResponse<List<ChatMessage>>, ChatMessageResponse<ChatMessage>, ChatMessage>.GetAllAsync()
        {
            try
            {
                var messages = await _chatMessageDataContext.GetAllAsync();
                return new ChatMessageResponse<List<ChatMessage>>
                {
                    Result = messages
                };
            }
            catch (UnauthorizedException)
            {
                return new ChatMessageResponse<List<ChatMessage>>
                {
                    Status = HttpStatusCode.Unauthorized,
                    Errors = new List<string> { "User is not authenticated or user ID is invalid. Please log in again to continue." }
                };
            }
            catch (Exception ex)
            {
                return new ChatMessageResponse<List<ChatMessage>>
                {
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        async Task<ChatMessageResponse<ChatMessage>> IEntityService<ChatMessageResponse<List<ChatMessage>>, ChatMessageResponse<ChatMessage>, ChatMessage>.GetByIdAsync(int id)
        {
            try
            {
                var message = await _chatMessageDataContext.GetByIdAsync(id);
                return new ChatMessageResponse<ChatMessage>
                {
                    Result = message
                };
            }
            catch (UnauthorizedException)
            {
                return new ChatMessageResponse<ChatMessage>
                {
                    Status = HttpStatusCode.Unauthorized,
                    Errors = new List<string> { "User is not authenticated or user ID is invalid. Please log in again to continue." }
                };
            }
            catch (Exception ex)
            {
                return new ChatMessageResponse<ChatMessage>
                {
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        async Task<ChatMessageResponse<List<ChatMessage>>> IChatMessageService<ChatMessageResponse<List<ChatMessage>>, ChatMessageResponse<ChatMessage>, ChatMessage>.GetBySessionIdAsync(int sessionId)
        {
            try
            {
                var messages = await _chatMessageDataContext.GetBySessionIdAsync(sessionId);
                return new ChatMessageResponse<List<ChatMessage>>
                {
                    Result = messages
                };
            }
            catch (UnauthorizedException)
            {
                return new ChatMessageResponse<List<ChatMessage>>
                {
                    Status = HttpStatusCode.Unauthorized,
                    Errors = new List<string> { "User is not authenticated or user ID is invalid. Please log in again to continue." }
                };
            }
            catch (Exception ex)
            {
                return new ChatMessageResponse<List<ChatMessage>>
                {
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        async Task<DeleteEntityResponse> IEntityService<ChatMessageResponse<List<ChatMessage>>, ChatMessageResponse<ChatMessage>, ChatMessage>.DeleteAsync(int id, Guid userId)
        {
            try
            {
                var user = await _userService.GetByUserIdAsync(userId.ToString());

                var message = await _chatMessageDataContext.DeleteAsync(id, user.Result.Id);
                return new DeleteEntityResponse
                {
                    Result = message,
                };
            }
            catch (UnauthorizedException)
            {
                return new DeleteEntityResponse
                {
                    Result = "User is not authenticated or user ID is invalid. Please log in again to continue.",
                    Status = HttpStatusCode.Unauthorized,
                    Errors = new List<string> { "User is not authenticated or user ID is invalid. Please log in again to continue." }
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
    }
}