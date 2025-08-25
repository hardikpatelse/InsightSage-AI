using InsightSage.Shared.Interfaces.Others;
using InsightSage.Shared.Interfaces.Services;
using InsightSage.Shared.Models.Entities;
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
    public class ChatModeController : ControllerBase
    {
        private readonly IChatModeService<ChatModeResponse<List<ChatMode>>, ChatModeResponse<ChatMode>, ChatMode> _chatModeService;
        private readonly IUserContext _userContext;

        public ChatModeController(
            IChatModeService<ChatModeResponse<List<ChatMode>>, ChatModeResponse<ChatMode>, ChatMode> chatModeService,
            IUserContext userContext)
        {
            _chatModeService = chatModeService;
            _userContext = userContext;
        }

        /// <summary>
        /// Get all chat modes
        /// </summary>
        [HttpGet]
        public async Task<ChatModeResponse<List<ChatMode>>> GetAllModes()
        {
            try
            {
                var result = await _chatModeService.GetAllAsync();
                return result;
            }
            catch (Exception ex)
            {
                return new ChatModeResponse<List<ChatMode>>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get all active chat modes
        /// </summary>
        [HttpGet("active")]
        public async Task<ChatModeResponse<List<ChatMode>>> GetAllActiveModes()
        {
            try
            {
                var result = await _chatModeService.GetAllActiveAsync();
                return result;
            }
            catch (Exception ex)
            {
                return new ChatModeResponse<List<ChatMode>>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get chat mode by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ChatModeResponse<ChatMode>> GetModeById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return new ChatModeResponse<ChatMode>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Invalid chat mode ID" }
                    };
                }

                var result = await _chatModeService.GetByIdAsync(id);
                return result;
            }
            catch (Exception ex)
            {
                return new ChatModeResponse<ChatMode>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get chat mode by slug
        /// </summary>
        [HttpGet("slug/{slug}")]
        public async Task<ChatModeResponse<ChatMode>> GetModeBySlug(string slug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug))
                {
                    return new ChatModeResponse<ChatMode>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Slug is required" }
                    };
                }

                var result = await _chatModeService.GetBySlugAsync(slug);
                return result;
            }
            catch (Exception ex)
            {
                return new ChatModeResponse<ChatMode>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get active chat mode by slug
        /// </summary>
        [HttpGet("active/slug/{slug}")]
        public async Task<ChatModeResponse<ChatMode>> GetActiveModeBySlug(string slug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug))
                {
                    return new ChatModeResponse<ChatMode>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Slug is required" }
                    };
                }

                var result = await _chatModeService.GetActiveBySlugAsync(slug);
                return result;
            }
            catch (Exception ex)
            {
                return new ChatModeResponse<ChatMode>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get chat mode by name
        /// </summary>
        [HttpGet("name/{name}")]
        public async Task<ChatModeResponse<ChatMode>> GetModeByName(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return new ChatModeResponse<ChatMode>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Name is required" }
                    };
                }

                var result = await _chatModeService.GetByNameAsync(name);
                return result;
            }
            catch (Exception ex)
            {
                return new ChatModeResponse<ChatMode>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Search chat modes by name
        /// </summary>
        [HttpGet("search")]
        public async Task<ChatModeResponse<List<ChatMode>>> SearchModes([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new ChatModeResponse<List<ChatMode>>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Search term is required" }
                    };
                }

                var result = await _chatModeService.SearchByNameAsync(searchTerm);
                return result;
            }
            catch (Exception ex)
            {
                return new ChatModeResponse<List<ChatMode>>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Search active chat modes by name
        /// </summary>
        [HttpGet("active/search")]
        public async Task<ChatModeResponse<List<ChatMode>>> SearchActiveModes([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new ChatModeResponse<List<ChatMode>>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Search term is required" }
                    };
                }

                var result = await _chatModeService.SearchActiveByNameAsync(searchTerm);
                return result;
            }
            catch (Exception ex)
            {
                return new ChatModeResponse<List<ChatMode>>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get all chat modes with icons
        /// </summary>
        [HttpGet("with-icons")]
        public async Task<ChatModeResponse<List<ChatMode>>> GetModesWithIcons()
        {
            try
            {
                var result = await _chatModeService.GetAllWithIconsAsync();
                return result;
            }
            catch (Exception ex)
            {
                return new ChatModeResponse<List<ChatMode>>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get all active chat modes with icons
        /// </summary>
        [HttpGet("active/with-icons")]
        public async Task<ChatModeResponse<List<ChatMode>>> GetActiveModesWithIcons()
        {
            try
            {
                var result = await _chatModeService.GetAllActiveWithIconsAsync();
                return result;
            }
            catch (Exception ex)
            {
                return new ChatModeResponse<List<ChatMode>>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Create a new chat mode
        /// </summary>
        [HttpPost]
        public async Task<AddUpdateEntityResponse> CreateMode([FromBody] ChatMode mode)
        {
            var result = new AddUpdateEntityResponse();
            try
            {
                if (mode == null)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Chat mode data is required");
                    return result;
                }

                if (string.IsNullOrWhiteSpace(mode.Name))
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Chat mode name is required");
                    return result;
                }

                result = await _chatModeService.AddUpdateAsync(mode);
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
        /// Update an existing chat mode
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<AddUpdateEntityResponse> UpdateMode(int id, [FromBody] ChatMode mode)
        {
            var result = new AddUpdateEntityResponse();
            try
            {
                if (mode == null)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Chat mode data is required");
                    return result;
                }

                if (id <= 0)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Invalid chat mode ID");
                    return result;
                }

                if (mode.Id != id)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Chat mode ID mismatch");
                    return result;
                }

                if (string.IsNullOrWhiteSpace(mode.Name))
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Chat mode name is required");
                    return result;
                }

                result = await _chatModeService.AddUpdateAsync(mode);
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
        /// Delete a chat mode
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<DeleteEntityResponse> DeleteMode(int id)
        {
            var result = new DeleteEntityResponse();
            try
            {
                if (id <= 0)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Invalid chat mode ID");
                    return result;
                }

                result = await _chatModeService.DeleteAsync(id, _userContext.UserId);
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
        /// Health check endpoint for the chat mode controller
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                controller = "ChatModeController",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            });
        }

        /// <summary>
        /// Debug endpoint to check authentication status
        /// </summary>
        [HttpGet("debug/auth")]
        [Authorize]
        public IActionResult AuthenticationDebug()
        {
            try
            {
                return Ok(new
                {
                    message = "Authentication successful for Chat Mode controller",
                    timestamp = DateTime.UtcNow,
                    userId = _userContext.UserId,
                    userEmail = _userContext.Email,
                    userName = _userContext.Name,
                    claims = HttpContext?.User?.Claims?.ToDictionary(c => c.Type, c => c.Value) ?? new Dictionary<string, string>()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to get authentication debug info",
                    message = ex.Message
                });
            }
        }
    }
}