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
    public class AIModelController : ControllerBase
    {
        private readonly IAIModelService<AIModelResponse<List<AIModel>>, AIModelResponse<AIModel>, AIModel> _aiModelService;
        private readonly IUserContext _userContext;

        public AIModelController(
            IAIModelService<AIModelResponse<List<AIModel>>, AIModelResponse<AIModel>, AIModel> aiModelService,
            IUserContext userContext)
        {
            _aiModelService = aiModelService;
            _userContext = userContext;
        }

        /// <summary>
        /// Get all AI models
        /// </summary>
        [HttpGet]
        public async Task<AIModelResponse<List<AIModel>>> GetAllModels()
        {
            try
            {
                var result = await _aiModelService.GetAllAsync();
                return result;
            }
            catch (Exception ex)
            {
                return new AIModelResponse<List<AIModel>>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get all active AI models
        /// </summary>
        [HttpGet("active")]
        public async Task<AIModelResponse<List<AIModel>>> GetAllActiveModels()
        {
            try
            {
                var result = await _aiModelService.GetAllActiveAsync();
                return result;
            }
            catch (Exception ex)
            {
                return new AIModelResponse<List<AIModel>>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get AI model by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<AIModelResponse<AIModel>> GetModelById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return new AIModelResponse<AIModel>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Invalid AI model ID" }
                    };
                }

                var result = await _aiModelService.GetByIdAsync(id);
                return result;
            }
            catch (Exception ex)
            {
                return new AIModelResponse<AIModel>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get AI model by slug
        /// </summary>
        [HttpGet("slug/{slug}")]
        public async Task<AIModelResponse<AIModel>> GetModelBySlug(string slug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug))
                {
                    return new AIModelResponse<AIModel>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Slug is required" }
                    };
                }

                var result = await _aiModelService.GetBySlugAsync(slug);
                return result;
            }
            catch (Exception ex)
            {
                return new AIModelResponse<AIModel>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get active AI model by slug
        /// </summary>
        [HttpGet("active/slug/{slug}")]
        public async Task<AIModelResponse<AIModel>> GetActiveModelBySlug(string slug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug))
                {
                    return new AIModelResponse<AIModel>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Slug is required" }
                    };
                }

                var result = await _aiModelService.GetActiveBySlugAsync(slug);
                return result;
            }
            catch (Exception ex)
            {
                return new AIModelResponse<AIModel>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get AI model by name
        /// </summary>
        [HttpGet("name/{name}")]
        public async Task<AIModelResponse<AIModel>> GetModelByName(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return new AIModelResponse<AIModel>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Name is required" }
                    };
                }

                var result = await _aiModelService.GetByNameAsync(name);
                return result;
            }
            catch (Exception ex)
            {
                return new AIModelResponse<AIModel>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Search AI models by name
        /// </summary>
        [HttpGet("search")]
        public async Task<AIModelResponse<List<AIModel>>> SearchModels([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new AIModelResponse<List<AIModel>>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Search term is required" }
                    };
                }

                var result = await _aiModelService.SearchByNameAsync(searchTerm);
                return result;
            }
            catch (Exception ex)
            {
                return new AIModelResponse<List<AIModel>>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Search active AI models by name
        /// </summary>
        [HttpGet("active/search")]
        public async Task<AIModelResponse<List<AIModel>>> SearchActiveModels([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new AIModelResponse<List<AIModel>>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Search term is required" }
                    };
                }

                var result = await _aiModelService.SearchActiveByNameAsync(searchTerm);
                return result;
            }
            catch (Exception ex)
            {
                return new AIModelResponse<List<AIModel>>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Create a new AI model
        /// </summary>
        [HttpPost]
        public async Task<AddUpdateEntityResponse> CreateModel([FromBody] AIModel model)
        {
            var result = new AddUpdateEntityResponse();
            try
            {
                if (model == null)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("AI model data is required");
                    return result;
                }

                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("AI model name is required");
                    return result;
                }

                result = await _aiModelService.AddUpdateAsync(model);
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
        /// Update an existing AI model
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<AddUpdateEntityResponse> UpdateModel(int id, [FromBody] AIModel model)
        {
            var result = new AddUpdateEntityResponse();
            try
            {
                if (model == null)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("AI model data is required");
                    return result;
                }

                if (id <= 0)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Invalid AI model ID");
                    return result;
                }

                if (model.Id != id)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("AI model ID mismatch");
                    return result;
                }

                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("AI model name is required");
                    return result;
                }

                result = await _aiModelService.AddUpdateAsync(model);
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
        /// Delete an AI model
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<DeleteEntityResponse> DeleteModel(int id)
        {
            var result = new DeleteEntityResponse();
            try
            {
                if (id <= 0)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Invalid AI model ID");
                    return result;
                }

                result = await _aiModelService.DeleteAsync(id, _userContext.UserId);
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
        /// Health check endpoint for the AI model controller
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                controller = "AIModelController",
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
                    message = "Authentication successful for AI Model controller",
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