using InsightSage.Shared.Exceptions;
using InsightSage.Shared.Interfaces.DataContexts;
using InsightSage.Shared.Interfaces.Others;
using InsightSage.Shared.Interfaces.Services;
using InsightSage.Shared.Interfaces.Services.Common;
using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Responses;
using InsightSage.Shared.Models.Responses.Common;
using System.Net;

namespace InsightSage.Application.Services
{
    public class ChatMessageService : IChatMessageService<ChatMessageResponse<List<ChatMessage>>, ChatMessageResponse<ChatMessage>, ChatMessage>
    {
        private readonly IChatMessageDataContext _chatMessageDataContext;
        private readonly IUserContext _userContext;
        private readonly IChatSessionService<ChatSessionResponse<List<ChatSession>>, ChatSessionResponse<ChatSession>, ChatSession> _chatSessionService;
        private readonly IUserService<UserResponse<List<User>>, UserResponse<User>, User> _userService;
        private readonly IAIModelService<AIModelResponse<List<AIModel>>, AIModelResponse<AIModel>, AIModel> _aiModelService;
        private readonly IChatModeService<ChatModeResponse<List<ChatMode>>, ChatModeResponse<ChatMode>, ChatMode> _chatModeService;

        public ChatMessageService(IUserContext userContext, 
            IChatMessageDataContext chatMessageDataContext,
            IChatSessionService<ChatSessionResponse<List<ChatSession>>, ChatSessionResponse<ChatSession>, ChatSession> chatSessionService,
            IUserService<UserResponse<List<User>>, UserResponse<User>, User> userService,
            IAIModelService<AIModelResponse<List<AIModel>>, AIModelResponse<AIModel>, AIModel> aiModelService,
            IChatModeService<ChatModeResponse<List<ChatMode>>, ChatModeResponse<ChatMode>, ChatMode> chatModeService)
        {
            _userContext = userContext;
            _chatMessageDataContext = chatMessageDataContext;
            _chatSessionService = chatSessionService;
            _userService = userService;
            _aiModelService = aiModelService;
            _chatModeService = chatModeService;
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

                // Validate that the chat session exists and belongs to the current user
                var sessionResponse = await _chatSessionService.GetByIdAsync(data.ChatSessionId.Value);
                if (sessionResponse.Result == null)
                {
                    return new AddUpdateEntityResponse
                    {
                        Status = HttpStatusCode.NotFound,
                        Errors = new List<string> { "Chat session not found" }
                    };
                }

                if (sessionResponse.Result.UserId != _userContext.UserId)
                {
                    return new AddUpdateEntityResponse
                    {
                        Status = HttpStatusCode.Forbidden,
                        Errors = new List<string> { "You don't have permission to add messages to this session" }
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

        /// <summary>
        /// Creates a new chat message with AI model and chat mode validation.
        /// If no session is provided, creates or uses a default session with the specified AI model and chat mode.
        /// </summary>
        /// <param name="data">Chat message data</param>
        /// <param name="aiModelId">ID of the AI model to use (optional if session already has one)</param>
        /// <param name="chatModeId">ID of the chat mode to use (optional if session already has one)</param>
        /// <returns>Response with the created message ID</returns>
        public async Task<AddUpdateEntityResponse> CreateMessageWithConfigurationAsync(ChatMessage data, int? aiModelId = null, int? chatModeId = null)
        {
            try
            {
                #region Validations
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

                // Validate AI Model if provided
                if (aiModelId.HasValue && aiModelId.Value > 0)
                {
                    var aiModelResponse = await _aiModelService.GetByIdAsync(aiModelId.Value);
                    if (aiModelResponse.Result == null)
                    {
                        return new AddUpdateEntityResponse
                        {
                            Status = HttpStatusCode.NotFound,
                            Errors = new List<string> { $"AI model with ID {aiModelId} not found" }
                        };
                    }

                    // Check if AI model is active
                    if (!aiModelResponse.Result.IsActive)
                    {
                        return new AddUpdateEntityResponse
                        {
                            Status = HttpStatusCode.BadRequest,
                            Errors = new List<string> { $"AI model '{aiModelResponse.Result.Name}' is not active" }
                        };
                    }
                }
                //Validate for NULL or 0 value of AI Model
                else
                {
                    return new AddUpdateEntityResponse
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { $"AI model is required" }
                    };
                }

                // Validate Chat Mode if provided
                if (chatModeId.HasValue && chatModeId.Value > 0)
                {
                    var chatModeResponse = await _chatModeService.GetByIdAsync(chatModeId.Value);
                    if (chatModeResponse.Result == null)
                    {
                        return new AddUpdateEntityResponse
                        {
                            Status = HttpStatusCode.NotFound,
                            Errors = new List<string> { $"Chat mode with ID {chatModeId} not found" }
                        };
                    }

                    // Check if chat mode is active
                    if (!chatModeResponse.Result.IsActive)
                    {
                        return new AddUpdateEntityResponse
                        {
                            Status = HttpStatusCode.BadRequest,
                            Errors = new List<string> { $"Chat mode '{chatModeResponse.Result.Name}' is not active" }
                        };
                    }
                }
                //Validate for NULL or 0 value of Chat Mode
                else
                {
                    return new AddUpdateEntityResponse
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { $"Chat mode is required" }
                    };
                }
                #endregion
                
                var chatSession = await CreateSessionIfNotValid(data, aiModelId, chatModeId);

                data.ChatSessionId = chatSession.Id;

                // Add the message
                var id = await _chatMessageDataContext.AddAsync(data);

                return new AddUpdateEntityResponse
                {
                    Result = id,
                    Status = HttpStatusCode.OK
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

        private async Task<ChatSession> CreateSessionIfNotValid(ChatMessage message, int? aiModelId = null, int? chatModeId = null)
        {
            var validSession = new ChatSession();
            // Handle default session logic when ChatSessionId is 0
            // Handle session logic
            if (message.ChatSessionId <= 0)
            {
                const string defaultSessionName = "Default Session";

                // Check if Default Session exists
                var defaultSessionResponse = await _chatSessionService.GetByUserIdAndExactNameAsync(defaultSessionName);

                if (defaultSessionResponse.Result != null)
                {
                    var session = defaultSessionResponse.Result;
                    bool sessionNeedsUpdate = false;

                    // Update session with AI model if provided and different
                    if (aiModelId.HasValue && session.AIModelId != aiModelId.Value)
                    {
                        session.AIModelId = aiModelId.Value;
                        sessionNeedsUpdate = true;
                    }

                    // Update session with chat mode if provided and different
                    if (chatModeId.HasValue && session.ChatModeId != chatModeId.Value)
                    {
                        session.ChatModeId = chatModeId.Value;
                        sessionNeedsUpdate = true;
                    }

                    // Update session if needed
                    if (sessionNeedsUpdate)
                    {
                        await _chatSessionService.AddUpdateAsync(session);
                    }

                    message.ChatSessionId = session.Id;
                    validSession = session;
                }
                else
                {
                    // Create new default session with AI model and chat mode
                    var newDefaultSession = new ChatSession
                    {
                        Name = defaultSessionName,
                        UserId = _userContext.UserId,
                        UniqueIdentity = Guid.NewGuid(),
                        AIModelId = aiModelId,
                        ChatModeId = chatModeId
                    };

                    var createSessionResponse = await _chatSessionService.AddUpdateAsync(newDefaultSession);

                    if (createSessionResponse.Result > 0)
                    {
                        message.ChatSessionId = createSessionResponse.Result;
                        validSession = newDefaultSession;
                        validSession.Id = createSessionResponse.Result;
                    }
                    else
                    {
                        throw new Exception("Failed to create default session");
                    }
                }
            }
            else
            {
                // Validate existing session
                var sessionResponse = await _chatSessionService.GetByIdAsync(message.ChatSessionId.Value);
                if (sessionResponse.Result == null)
                {
                    throw new Exception("Chat session not found");
                }

                if (sessionResponse.Result.UserId != _userContext.UserId)
                {
                    throw new Exception("You don't have permission to add messages to this session");
                }

                // Update session with AI model and chat mode if provided
                var session = sessionResponse.Result;
                bool sessionNeedsUpdate = false;

                if (aiModelId.HasValue && session.AIModelId != aiModelId.Value)
                {
                    session.AIModelId = aiModelId.Value;
                    sessionNeedsUpdate = true;
                }

                if (chatModeId.HasValue && session.ChatModeId != chatModeId.Value)
                {
                    session.ChatModeId = chatModeId.Value;
                    sessionNeedsUpdate = true;
                }

                if (sessionNeedsUpdate)
                {
                    await _chatSessionService.AddUpdateAsync(session);
                    validSession = session;
                }
            }

            return validSession;
        }
    }
}