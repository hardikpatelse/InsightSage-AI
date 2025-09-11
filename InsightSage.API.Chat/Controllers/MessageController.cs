using InsightSage.Application.Services;
using InsightSage.Application.Services.AI;
using InsightSage.Shared.Interfaces.Others;
using InsightSage.Shared.Interfaces.Services;
using InsightSage.Shared.Models.Custom;
using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Requests;
using InsightSage.Shared.Models.Responses;
using InsightSage.Shared.Models.Responses.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace InsightSage.API.Chat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IChatMessageService<ChatMessageResponse<List<ChatMessage>>, ChatMessageResponse<ChatMessage>, ChatMessage> _chatMessageService;
        private readonly IUserContext _userContext;

        public MessageController(IChatMessageService<ChatMessageResponse<List<ChatMessage>>, ChatMessageResponse<ChatMessage>, ChatMessage> chatMessageService, IUserContext userContext)
        {
            _chatMessageService = chatMessageService;
            _userContext = userContext;
        }

        /// <summary>
        /// Get all chat messages
        /// </summary>
        [HttpGet]
        public async Task<ChatMessageResponse<List<ChatMessage>>> GetAllMessages()
        {
            try
            {
                var result = await _chatMessageService.GetAllAsync();
                return result;
            }
            catch (Exception ex)
            {
                return new ChatMessageResponse<List<ChatMessage>>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get chat message by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ChatMessageResponse<ChatMessage>> GetMessageById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return new ChatMessageResponse<ChatMessage>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Invalid message ID" }
                    };
                }

                var result = await _chatMessageService.GetByIdAsync(id);
                return result;
            }
            catch (Exception ex)
            {
                return new ChatMessageResponse<ChatMessage>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get all chat messages for a specific session
        /// </summary>
        [HttpGet("session/{sessionId:int}")]
        public async Task<ChatMessageResponse<List<ChatMessage>>> GetMessagesBySessionId(int sessionId)
        {
            try
            {
                if (sessionId <= 0)
                {
                    return new ChatMessageResponse<List<ChatMessage>>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Invalid session ID" }
                    };
                }

                var result = await _chatMessageService.GetBySessionIdAsync(sessionId);
                return result;
            }
            catch (Exception ex)
            {
                return new ChatMessageResponse<List<ChatMessage>>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Create a new chat message with optional AI model and chat mode configuration
        /// </summary>
        //[HttpPost]
        //public async Task<AddUpdateEntityResponse> CreateMessage([FromBody] MessageWithConfigRequest request)
        //{
        //    var result = new AddUpdateEntityResponse();
        //    try
        //    {
        //        // Cast to ChatMessageService to access the new method with configuration
        //        if (_chatMessageService is ChatMessageService concreteService)
        //        {
        //            result = await concreteService.CreateMessageWithConfigurationAsync(
        //                request.Message, 
        //                request.AIModelId, 
        //                request.ChatModeId);
        //        }
        //        else
        //        {
        //            // Fallback to regular method if concrete service is not available
        //            result = await _chatMessageService.AddUpdateAsync(request.Message);
        //        }

        //        // If session configuration was provided and message creation was successful,
        //        // update session configuration in background task (non-blocking)
        //        if (result.Status == HttpStatusCode.OK && result.Result > 0 && 
        //            (request.AIModelId.HasValue || request.ChatModeId.HasValue) &&
        //            request.Message.ChatSessionId.HasValue && request.Message.ChatSessionId > 0)
        //        {
        //            // Fire-and-forget background task to update session configuration
        //            _ = Task.Run(async () =>
        //            {
        //                try
        //                {
        //                    await UpdateSessionConfigurationAsync(
        //                        request.Message.ChatSessionId.Value, 
        //                        request.AIModelId, 
        //                        request.ChatModeId);
        //                }
        //                catch (Exception ex)
        //                {
        //                    // Log the error but don't affect the main response
        //                    System.Diagnostics.Debug.WriteLine($"Background session update failed: {ex.Message}");
        //                }
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Status = HttpStatusCode.InternalServerError;
        //        result.Errors.Add(ex.GetBaseException().Message);
        //        result.ExceptionDetails = ex.ToString();
        //    }
        //    return result;
        //}

        /// <summary>
        /// Background helper method to update session configuration
        /// </summary>
        /// <param name="sessionId">Session ID to update</param>
        /// <param name="aiModelId">AI model ID to set (optional)</param>
        /// <param name="chatModeId">Chat mode ID to set (optional)</param>
        /// <returns>Task for background execution</returns>
        private async Task UpdateSessionConfigurationAsync(int sessionId, int? aiModelId, int? chatModeId)
        {
            try
            {
                // Get session service from HTTP context if available
                var sessionService = HttpContext.RequestServices
                    .GetService<IChatSessionService<ChatSessionResponse<List<ChatSession>>, ChatSessionResponse<ChatSession>, ChatSession>>();

                if (sessionService is ChatSessionService concreteSessionService)
                {
                    await concreteSessionService.UpdateSessionConfigurationAsync(
                        sessionId, 
                        aiModelId, 
                        chatModeId);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't propagate - this is a background operation
                System.Diagnostics.Debug.WriteLine($"Session configuration update failed for session {sessionId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing chat message
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<AddUpdateEntityResponse> UpdateMessage(int id, [FromBody] ChatMessage message)
        {
            var result = new AddUpdateEntityResponse();
            try
            {
                if (message == null)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Message data is required");
                    return result;
                }

                if (id <= 0)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Invalid message ID");
                    return result;
                }

                if (message.Id != id)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Message ID mismatch");
                    return result;
                }

                if (string.IsNullOrWhiteSpace(message.Role))
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Message role is required");
                    return result;
                }

                if (string.IsNullOrWhiteSpace(message.Content))
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Message content is required");
                    return result;
                }

                if (message.ChatSessionId <= 0)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Valid chat session ID is required");
                    return result;
                }

                result = await _chatMessageService.AddUpdateAsync(message);
            }
            catch (Exception ex)
            {
                result.Status = HttpStatusCode.InternalServerError;
                result.Errors.Add(ex.GetBaseException().Message);
                result.ExceptionDetails = ex.ToString();
            }
            return result;
        }

        /// <summary>
        /// Delete a chat message
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<DeleteEntityResponse> DeleteMessage(int id)
        {
            var result = new DeleteEntityResponse();
            try
            {
                if (id <= 0)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Invalid message ID");
                    return result;
                }

                result = await _chatMessageService.DeleteAsync(id, _userContext.UserId);
            }
            catch (Exception ex)
            {
                result.Status = HttpStatusCode.InternalServerError;
                result.Errors.Add(ex.GetBaseException().Message);
                result.ExceptionDetails = ex.ToString();
            }
            return result;
        }

        /// <summary>
        /// Health check endpoint for the message controller
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                controller = "MessageController",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            });
        }

        /// <summary>
        /// Diagnostic endpoint to check authentication status and claims
        /// </summary>
        [HttpGet("debug/auth")]
        [Authorize]
        public IActionResult AuthenticationDebug()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"]
                    .FirstOrDefault()?.Split(" ").Last();

                var authInfo = new
                {
                    hasHttpContext = HttpContext != null,
                    hasUser = HttpContext?.User != null,
                    isAuthenticated = HttpContext?.User?.Identity?.IsAuthenticated ?? false,
                    authenticationType = HttpContext?.User?.Identity?.AuthenticationType,
                    userName = HttpContext?.User?.Identity?.Name,
                    hasToken = !string.IsNullOrEmpty(token),
                    tokenLength = token?.Length ?? 0,
                    tokenStart = token?.Length > 20 ? token.Substring(0, 20) + "..." : token,

                    // Claims information
                    allClaims = HttpContext?.User?.Claims?.ToDictionary(c => c.Type, c => c.Value) ?? new Dictionary<string, string>(),

                    // Specific claims we're looking for
                    specificClaims = new
                    {
                        oid = HttpContext?.User?.FindFirst("oid")?.Value,
                        nameIdentifier = HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        sub = HttpContext?.User?.FindFirst("sub")?.Value,
                        email = HttpContext?.User?.FindFirst("emails")?.Value ?? HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value,
                        name = HttpContext?.User?.FindFirst("name")?.Value,
                        tenantId = HttpContext?.User?.FindFirst("tid")?.Value
                    },

                    // UserContext information
                    userContextInfo = new
                    {
                        canAccessUserId = false,
                        userIdError = (string?)null,
                        email = _userContext.Email,
                        name = _userContext.Name,
                        tenantId = _userContext.TenantId
                    }
                };

                // Try to access UserId to see if it throws an exception
                try
                {
                    var userId = _userContext.UserId;
                    authInfo = authInfo with
                    {
                        userContextInfo = authInfo.userContextInfo with
                        {
                            canAccessUserId = true,
                            userIdError = null
                        }
                    };
                }
                catch (Exception ex)
                {
                    authInfo = authInfo with
                    {
                        userContextInfo = authInfo.userContextInfo with
                        {
                            canAccessUserId = false,
                            userIdError = ex.Message
                        }
                    };
                }

                return Ok(new
                {
                    message = "Authentication debug information",
                    timestamp = DateTime.UtcNow,
                    authenticationInfo = authInfo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to get authentication debug info",
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Create a new chat message with AI processing
        /// </summary>
        [HttpPost("with-ai")]
        public async Task<AddUpdateEntityResponse> CreateMessageWithAI([FromBody] MessageWithAIRequest request)
        {
            var result = new AddUpdateEntityResponse();
            try
            {
                if (request?.Message == null)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Message data is required");
                    return result;
                }

                if (string.IsNullOrWhiteSpace(request.Message.Role))
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Message role is required");
                    return result;
                }

                if (string.IsNullOrWhiteSpace(request.Message.Content))
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Message content is required");
                    return result;
                }

                // Save user message first
                if (request.Message.ChatSessionId == null || request.Message.ChatSessionId <= 0)
                {
                    result = await _chatMessageService.CreateMessageWithConfigurationAsync(
                        request.Message, 
                        request.AIModelId, 
                        request.ChatModeId);
                }
                else
                {
                    result = await _chatMessageService.AddUpdateAsync(request.Message);
                }

                // If message was saved successfully and AI processing is requested
                if (result.Result > 0 && request.ProcessWithAI)
                {
                    // Get AI processing service and queue the request
                    var aiProcessingService = HttpContext.RequestServices.GetService<AIProcessingService>();
                    if (aiProcessingService != null)
                    {
                        var aiRequest = new AIProcessingRequest
                        {
                            SessionId = request.Message.ChatSessionId ?? 0,
                            UserMessageId = result.Result,
                            UserId = _userContext.UserId,
                            UserMessage = request.Message.Content,
                            PlatformSlug = request.PlatformSlug,
                            Model = request.Model,
                            MaxTokens = request.MaxTokens,
                            Temperature = request.Temperature
                        };

                        aiProcessingService.QueueProcessing(aiRequest);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Status = HttpStatusCode.InternalServerError;
                result.Errors.Add(ex.GetBaseException().Message);
                result.ExceptionDetails = ex.ToString();
                return result;
            }
        }
    }
}