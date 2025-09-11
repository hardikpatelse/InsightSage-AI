using InsightSage.Application.Services;
using InsightSage.Shared.Interfaces.Others;
using InsightSage.Shared.Interfaces.Services;
using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Requests;
using InsightSage.Shared.Models.Responses;
using InsightSage.Shared.Models.Responses.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace InsightSage.API.Chat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SessionController : ControllerBase
    {
        private readonly IChatSessionService<ChatSessionResponse<List<ChatSession>>, ChatSessionResponse<ChatSession>, ChatSession> _chatSessionService;
        private readonly IUserContext _userContext;

        public SessionController(IChatSessionService<ChatSessionResponse<List<ChatSession>>, ChatSessionResponse<ChatSession>, ChatSession> chatSessionService, IUserContext userContext)
        {
            _chatSessionService = chatSessionService;
            _userContext = userContext;
        }

        /// <summary>
        /// Get all chat sessions
        /// </summary>
        [HttpGet]
        public async Task<ChatSessionResponse<List<ChatSession>>> GetAllSessions()
        {
            try
            {
                var result = await _chatSessionService.GetAllAsync();
                return result;
            }
            catch (Exception ex)
            {
                return new ChatSessionResponse<List<ChatSession>>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get chat session by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ChatSessionResponse<ChatSession>> GetSessionById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return new ChatSessionResponse<ChatSession>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Invalid session ID" }
                    };
                }

                var result = await _chatSessionService.GetByIdAsync(id);
                return result;
            }
            catch (Exception ex)
            {
                return new ChatSessionResponse<ChatSession>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get chat session by unique identity
        /// </summary>
        [HttpGet("unique/{uniqueIdentity:guid}")]
        public async Task<ChatSessionResponse<ChatSession>> GetSessionByUniqueIdentity(Guid uniqueIdentity)
        {
            try
            {
                if (uniqueIdentity == Guid.Empty)
                {
                    return new ChatSessionResponse<ChatSession>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Invalid unique identity" }
                    };
                }

                var result = await _chatSessionService.GetByUniqueIdentityAsync(uniqueIdentity);
                return result;
            }
            catch (Exception ex)
            {
                return new ChatSessionResponse<ChatSession>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get all chat sessions for the current user
        /// </summary>
        [HttpGet("user")]
        public async Task<ChatSessionResponse<List<ChatSession>>> GetUserSessions()
        {
            try
            {
                var result = await _chatSessionService.GetAllByUserIdAsync();
                return result;
            }
            catch (Exception ex)
            {
                return new ChatSessionResponse<List<ChatSession>>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Search chat sessions by name for the current user
        /// </summary>
        [HttpGet("search")]
        public async Task<ChatSessionResponse<List<ChatSession>>> SearchSessions([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return new ChatSessionResponse<List<ChatSession>>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Search name is required" }
                    };
                }

                var result = await _chatSessionService.GetSessionsByUserAndNameAsync(name);
                return result;
            }
            catch (Exception ex)
            {
                return new ChatSessionResponse<List<ChatSession>>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Create a new chat session
        /// </summary>
        [HttpPost]
        public async Task<AddUpdateEntityResponse> CreateSession([FromBody] ChatSession session)
        {
            var result = new AddUpdateEntityResponse();
            try
            {
                if (session == null)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Session data is required");
                    return result;
                }

                if (string.IsNullOrWhiteSpace(session.Name))
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Session name is required");
                    return result;
                }

                result = await _chatSessionService.AddUpdateAsync(session);
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
        /// Create a new chat session with AI model and chat mode configuration
        /// </summary>
        [HttpPost("with-config")]
        public async Task<AddUpdateEntityResponse> CreateSessionWithConfiguration([FromBody] SessionWithConfigRequest request)
        {
            var result = new AddUpdateEntityResponse();
            try
            {
                if (request == null)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Request data is required");
                    return result;
                }

                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Session name is required");
                    return result;
                }

                // Cast to ChatSessionService to access the new method
                if (_chatSessionService is ChatSessionService concreteService)
                {
                    result = await concreteService.CreateSessionWithConfigurationAsync(
                        request.Name,
                        request.AIModelId,
                        request.ChatModeId);
                }
                else
                {
                    // Fallback to regular method
                    var session = new ChatSession
                    {
                        Name = request.Name,
                        AIModelId = request.AIModelId,
                        ChatModeId = request.ChatModeId
                    };
                    result = await _chatSessionService.AddUpdateAsync(session);
                }
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
        /// Update chat session configuration (AI model and/or chat mode)
        /// </summary>
        [HttpPatch("{id:int}/config")]
        public async Task<AddUpdateEntityResponse> UpdateSessionConfiguration(int id, [FromBody] SessionWithConfigRequest request)
        {
            var result = new AddUpdateEntityResponse();
            try
            {
                if (request == null)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Request data is required");
                    return result;
                }

                if (id <= 0)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Invalid session ID");
                    return result;
                }

                // Cast to ChatSessionService to access the new method
                if (_chatSessionService is ChatSessionService concreteService)
                {
                    result = await concreteService.UpdateSessionConfigurationAsync(
                        id,
                        request.AIModelId,
                        request.ChatModeId);
                }
                else
                {
                    result.Status = HttpStatusCode.InternalServerError;
                    result.Errors.Add("Configuration update is not supported");
                }
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
        /// Update an existing chat session
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<AddUpdateEntityResponse> UpdateSession(int id, [FromBody] ChatSession session)
        {
            var result = new AddUpdateEntityResponse();
            try
            {
                if (session == null)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Session data is required");
                    return result;
                }

                if (id <= 0)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Invalid session ID");
                    return result;
                }

                if (session.Id != id)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Session ID mismatch");
                    return result;
                }

                if (string.IsNullOrWhiteSpace(session.Name))
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Session name is required");
                    return result;
                }

                result = await _chatSessionService.AddUpdateAsync(session);
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
        /// Delete a chat session
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<DeleteEntityResponse> DeleteSession(int id)
        {
            var result = new DeleteEntityResponse();
            try
            {
                if (id <= 0)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Invalid session ID");
                    return result;
                }

                result = await _chatSessionService.DeleteAsync(id, _userContext.UserId);
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
        /// Health check endpoint for the session controller
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                controller = "SessionController",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            });
        }
    }
}
