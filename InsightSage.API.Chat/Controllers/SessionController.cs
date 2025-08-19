using InsightSage.Shared.Interfaces.Others;
using InsightSage.Shared.Interfaces.Services;
using InsightSage.Shared.Models.Entities;
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
        /// Get all chat sessions for a specific user
        /// </summary>
        [HttpGet("user")]
        public async Task<ChatSessionResponse<List<ChatSession>>> GetSessionsByUserId()
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
        /// Search chat sessions by user ID and session name
        /// </summary>
        [HttpGet("search/user")]
        public async Task<ChatSessionResponse<List<ChatSession>>> SearchSessionsByUserAndName([FromQuery] string name)
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
        /// Update an existing chat session
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<AddUpdateEntityResponse> UpdateSession(int id, [FromBody] ChatSession session)
        {
            var result = new AddUpdateEntityResponse();
            try
            {
                //if (session == null)
                //{
                //    return BadRequest(new ChatSessionResponse<ChatSession>
                //    {
                //        Status = HttpStatusCode.BadRequest,
                //        Errors = new List<string> { "Session data is required" }
                //    });
                //}

                //if (id <= 0)
                //{
                //    return BadRequest(new ChatSessionResponse<ChatSession>
                //    {
                //        Status = HttpStatusCode.BadRequest,
                //        Errors = new List<string> { "Invalid session ID" }
                //    });
                //}

                //if (session.Id != id)
                //{
                //    return BadRequest(new ChatSessionResponse<ChatSession>
                //    {
                //        Status = HttpStatusCode.BadRequest,
                //        Errors = new List<string> { "Session ID mismatch" }
                //    });
                //}

                //if (string.IsNullOrWhiteSpace(session.Name))
                //{
                //    return BadRequest(new ChatSessionResponse<ChatSession>
                //    {
                //        Status = HttpStatusCode.BadRequest,
                //        Errors = new List<string> { "Session name is required" }
                //    });
                //}

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
        public async Task<DeleteEntityResponse> DeleteSession(int id, [FromQuery] int modifiedBy = 0)
        {
            var result = new DeleteEntityResponse();
            try
            {
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
