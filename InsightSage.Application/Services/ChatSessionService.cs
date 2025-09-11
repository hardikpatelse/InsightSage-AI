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
        private readonly IAIModelService<AIModelResponse<List<AIModel>>, AIModelResponse<AIModel>, AIModel> _aiModelService;
        private readonly IChatModeService<ChatModeResponse<List<ChatMode>>, ChatModeResponse<ChatMode>, ChatMode> _chatModeService;

        public ChatSessionService(IUserContext userContext, 
            IChatSessionDataContext chatSessionDataContext, 
            IUserService<UserResponse<List<User>>, UserResponse<User>, User> userService,
            IAIModelService<AIModelResponse<List<AIModel>>, AIModelResponse<AIModel>, AIModel> aiModelService,
            IChatModeService<ChatModeResponse<List<ChatMode>>, ChatModeResponse<ChatMode>, ChatMode> chatModeService)
        {
            _userContext = userContext;
            _chatSessionDataContext = chatSessionDataContext;
            _userService = userService;
            _aiModelService = aiModelService;
            _chatModeService = chatModeService;
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

                // Basic validation for updates
                if (data.Id > 0)
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

                // Validate AI Model if provided
                if (data.AIModelId.HasValue && data.AIModelId.Value > 0)
                {
                    var aiModelResponse = await _aiModelService.GetByIdAsync(data.AIModelId.Value);
                    if (aiModelResponse.Result == null)
                    {
                        return new AddUpdateEntityResponse
                        {
                            Status = HttpStatusCode.NotFound,
                            Errors = new List<string> { $"AI model with ID {data.AIModelId} not found" }
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

                // Validate Chat Mode if provided
                if (data.ChatModeId.HasValue && data.ChatModeId.Value > 0)
                {
                    var chatModeResponse = await _chatModeService.GetByIdAsync(data.ChatModeId.Value);
                    if (chatModeResponse.Result == null)
                    {
                        return new AddUpdateEntityResponse
                        {
                            Status = HttpStatusCode.NotFound,
                            Errors = new List<string> { $"Chat mode with ID {data.ChatModeId} not found" }
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

        /// <summary>
        /// Creates a new chat session with AI model and chat mode validation
        /// </summary>
        /// <param name="name">Session name</param>
        /// <param name="aiModelId">AI model ID (optional)</param>
        /// <param name="chatModeId">Chat mode ID (optional)</param>
        /// <returns>Response with created session ID</returns>
        public async Task<AddUpdateEntityResponse> CreateSessionWithConfigurationAsync(string name, int? aiModelId = null, int? chatModeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return new AddUpdateEntityResponse
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Session name is required" }
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

                    if (!aiModelResponse.Result.IsActive)
                    {
                        return new AddUpdateEntityResponse
                        {
                            Status = HttpStatusCode.BadRequest,
                            Errors = new List<string> { $"AI model '{aiModelResponse.Result.Name}' is not active" }
                        };
                    }
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

                    if (!chatModeResponse.Result.IsActive)
                    {
                        return new AddUpdateEntityResponse
                        {
                            Status = HttpStatusCode.BadRequest,
                            Errors = new List<string> { $"Chat mode '{chatModeResponse.Result.Name}' is not active" }
                        };
                    }
                }

                var session = new ChatSession
                {
                    Name = name,
                    UserId = _userContext.UserId,
                    UniqueIdentity = Guid.NewGuid(),
                    AIModelId = aiModelId,
                    ChatModeId = chatModeId
                };

                var id = await _chatSessionDataContext.AddAsync(session);

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
                    Errors = new List<string> { ex.Message },
                    ExceptionDetails = ex.ToString()
                };
            }
        }

        /// <summary>
        /// Updates session configuration (AI model and/or chat mode)
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <param name="aiModelId">New AI model ID (optional)</param>
        /// <param name="chatModeId">New chat mode ID (optional)</param>
        /// <returns>Update response</returns>
        public async Task<AddUpdateEntityResponse> UpdateSessionConfigurationAsync(int sessionId, int? aiModelId = null, int? chatModeId = null)
        {
            try
            {
                if (sessionId <= 0)
                {
                    return new AddUpdateEntityResponse
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Invalid session ID" }
                    };
                }

                // Get existing session
                var sessionResponse = await ((IEntityService<ChatSessionResponse<List<ChatSession>>, ChatSessionResponse<ChatSession>, ChatSession>)this).GetByIdAsync(sessionId);
                if (sessionResponse.Result == null)
                {
                    return new AddUpdateEntityResponse
                    {
                        Status = HttpStatusCode.NotFound,
                        Errors = new List<string> { "Chat session not found" }
                    };
                }

                // Verify ownership
                if (sessionResponse.Result.UserId != _userContext.UserId)
                {
                    return new AddUpdateEntityResponse
                    {
                        Status = HttpStatusCode.Forbidden,
                        Errors = new List<string> { "You don't have permission to update this session" }
                    };
                }

                var session = sessionResponse.Result;
                bool hasChanges = false;

                // Update AI Model if provided
                if (aiModelId.HasValue)
                {
                    if (aiModelId.Value > 0)
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

                        if (!aiModelResponse.Result.IsActive)
                        {
                            return new AddUpdateEntityResponse
                            {
                                Status = HttpStatusCode.BadRequest,
                                Errors = new List<string> { $"AI model '{aiModelResponse.Result.Name}' is not active" }
                        };
                        }
                    }

                    if (session.AIModelId != aiModelId.Value)
                    {
                        session.AIModelId = aiModelId.Value > 0 ? aiModelId.Value : null;
                        hasChanges = true;
                    }
                }

                // Update Chat Mode if provided
                if (chatModeId.HasValue)
                {
                    if (chatModeId.Value > 0)
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

                        if (!chatModeResponse.Result.IsActive)
                        {
                            return new AddUpdateEntityResponse
                            {
                                Status = HttpStatusCode.BadRequest,
                                Errors = new List<string> { $"Chat mode '{chatModeResponse.Result.Name}' is not active" }
                        };
                        }
                    }

                    if (session.ChatModeId != chatModeId.Value)
                    {
                        session.ChatModeId = chatModeId.Value > 0 ? chatModeId.Value : null;
                        hasChanges = true;
                    }
                }

                if (hasChanges)
                {
                    var id = await _chatSessionDataContext.UpdateAsync(session);
                    return new AddUpdateEntityResponse
                    {
                        Result = id,
                        Status = HttpStatusCode.OK
                    };
                }

                return new AddUpdateEntityResponse
                {
                    Result = sessionId,
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