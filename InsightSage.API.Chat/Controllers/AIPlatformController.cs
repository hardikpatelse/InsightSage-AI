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
    public class AIPlatformController : ControllerBase
    {
        private readonly IAIPlatformService<AIPlatformResponse<List<AIPlatform>>, AIPlatformResponse<AIPlatform>, AIPlatform> _aiPlatformService;
        private readonly IUserContext _userContext;

        public AIPlatformController(
            IAIPlatformService<AIPlatformResponse<List<AIPlatform>>, AIPlatformResponse<AIPlatform>, AIPlatform> aiPlatformService, 
            IUserContext userContext)
        {
            _aiPlatformService = aiPlatformService;
            _userContext = userContext;
        }

        /// <summary>
        /// Get all AI platforms
        /// </summary>
        [HttpGet]
        public async Task<AIPlatformResponse<List<AIPlatform>>> GetAllPlatforms()
        {
            try
            {
                var result = await _aiPlatformService.GetAllAsync();
                return result;
            }
            catch (Exception ex)
            {
                return new AIPlatformResponse<List<AIPlatform>>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get all active AI platforms
        /// </summary>
        [HttpGet("active")]
        public async Task<AIPlatformResponse<List<AIPlatform>>> GetActivePlatforms()
        {
            try
            {
                var result = await _aiPlatformService.GetAllActiveAsync();
                return result;
            }
            catch (Exception ex)
            {
                return new AIPlatformResponse<List<AIPlatform>>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get AI platform by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<AIPlatformResponse<AIPlatform>> GetPlatformById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return new AIPlatformResponse<AIPlatform>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Invalid platform ID" }
                    };
                }

                var result = await _aiPlatformService.GetByIdAsync(id);
                return result;
            }
            catch (Exception ex)
            {
                return new AIPlatformResponse<AIPlatform>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get AI platform by slug
        /// </summary>
        [HttpGet("slug/{slug}")]
        public async Task<AIPlatformResponse<AIPlatform>> GetPlatformBySlug(string slug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug))
                {
                    return new AIPlatformResponse<AIPlatform>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Slug is required" }
                    };
                }

                var result = await _aiPlatformService.GetBySlugAsync(slug);
                return result;
            }
            catch (Exception ex)
            {
                return new AIPlatformResponse<AIPlatform>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get active AI platform by slug
        /// </summary>
        [HttpGet("active/slug/{slug}")]
        public async Task<AIPlatformResponse<AIPlatform>> GetActivePlatformBySlug(string slug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug))
                {
                    return new AIPlatformResponse<AIPlatform>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Slug is required" }
                    };
                }

                var result = await _aiPlatformService.GetActiveBySlugAsync(slug);
                return result;
            }
            catch (Exception ex)
            {
                return new AIPlatformResponse<AIPlatform>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Search AI platforms by name
        /// </summary>
        [HttpGet("search")]
        public async Task<AIPlatformResponse<List<AIPlatform>>> SearchPlatforms([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new AIPlatformResponse<List<AIPlatform>>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Search term is required" }
                    };
                }

                var result = await _aiPlatformService.SearchByNameAsync(searchTerm);
                return result;
            }
            catch (Exception ex)
            {
                return new AIPlatformResponse<List<AIPlatform>>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Search active AI platforms by name
        /// </summary>
        [HttpGet("active/search")]
        public async Task<AIPlatformResponse<List<AIPlatform>>> SearchActivePlatforms([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new AIPlatformResponse<List<AIPlatform>>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Search term is required" }
                    };
                }

                var result = await _aiPlatformService.SearchActiveByNameAsync(searchTerm);
                return result;
            }
            catch (Exception ex)
            {
                return new AIPlatformResponse<List<AIPlatform>>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Test connectivity to a platform
        /// </summary>
        [HttpGet("{slug}/test-connectivity")]
        public async Task<IActionResult> TestPlatformConnectivity(string slug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug))
                {
                    return BadRequest(new { error = "Platform slug is required" });
                }

                var isConnected = await _aiPlatformService.TestPlatformConnectivityAsync(slug);
                
                return Ok(new 
                { 
                    slug = slug,
                    isConnected = isConnected,
                    message = isConnected ? "Platform is accessible" : "Platform is not accessible",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Error testing platform connectivity",
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Create a new AI platform
        /// </summary>
        [HttpPost]
        public async Task<AddUpdateEntityResponse> CreatePlatform([FromBody] AIPlatform platform)
        {
            var result = new AddUpdateEntityResponse();
            try
            {
                if (platform == null)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Platform data is required");
                    return result;
                }

                if (string.IsNullOrWhiteSpace(platform.Name))
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Platform name is required");
                    return result;
                }

                if (string.IsNullOrWhiteSpace(platform.ApiBaseUrl))
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("API base URL is required");
                    return result;
                }

                result = await _aiPlatformService.AddUpdateAsync(platform);
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
        /// Update an existing AI platform
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<AddUpdateEntityResponse> UpdatePlatform(int id, [FromBody] AIPlatform platform)
        {
            var result = new AddUpdateEntityResponse();
            try
            {
                if (platform == null)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Platform data is required");
                    return result;
                }

                if (id <= 0)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Invalid platform ID");
                    return result;
                }

                if (platform.Id != id)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Platform ID mismatch");
                    return result;
                }

                if (string.IsNullOrWhiteSpace(platform.Name))
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Platform name is required");
                    return result;
                }

                if (string.IsNullOrWhiteSpace(platform.ApiBaseUrl))
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("API base URL is required");
                    return result;
                }

                result = await _aiPlatformService.AddUpdateAsync(platform);
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
        /// Delete an AI platform
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<DeleteEntityResponse> DeletePlatform(int id)
        {
            var result = new DeleteEntityResponse();
            try
            {
                if (id <= 0)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Invalid platform ID");
                    return result;
                }

                result = await _aiPlatformService.DeleteAsync(id, _userContext.UserId);
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
        /// Health check endpoint for the AI platform controller
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                controller = "AIPlatformController",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            });
        }
    }
}