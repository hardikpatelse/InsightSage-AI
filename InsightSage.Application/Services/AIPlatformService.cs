using InsightSage.Shared.Interfaces.DataContexts;
using InsightSage.Shared.Interfaces.Services;
using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Responses;
using InsightSage.Shared.Models.Responses.Common;
using System.Net;

namespace InsightSage.Application.Services
{
    public class AIPlatformService : IAIPlatformService<AIPlatformResponse<List<AIPlatform>>, AIPlatformResponse<AIPlatform>, AIPlatform>
    {
        private readonly IAIPlatformDataContext _aiPlatformDataContext;

        public AIPlatformService(IAIPlatformDataContext aiPlatformDataContext)
        {
            _aiPlatformDataContext = aiPlatformDataContext;
        }

        public async Task<AddUpdateEntityResponse> AddUpdateAsync(AIPlatform data)
        {
            try
            {
                if (data == null)
                {
                    return new AddUpdateEntityResponse
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "AI platform data is required" }
                    };
                }

                if (string.IsNullOrWhiteSpace(data.Name))
                {
                    return new AddUpdateEntityResponse
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "AI platform name is required" }
                    };
                }

                if (string.IsNullOrWhiteSpace(data.ApiBaseUrl))
                {
                    return new AddUpdateEntityResponse
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "API base URL is required" }
                    };
                }

                int id;
                if (data.Id == 0)
                {
                    // Add new AI platform
                    id = await _aiPlatformDataContext.AddAsync(data);
                }
                else
                {
                    // Update existing AI platform
                    id = await _aiPlatformDataContext.UpdateAsync(data);
                }

                return new AddUpdateEntityResponse
                {
                    Result = id,
                    Status = HttpStatusCode.OK
                };
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                return new AddUpdateEntityResponse
                {
                    Status = HttpStatusCode.Conflict,
                    Errors = new List<string> { ex.Message }
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

        public async Task<DeleteEntityResponse> DeleteAsync(int id, Guid userId)
        {
            try
            {
                if (id <= 0)
                {
                    return new DeleteEntityResponse
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Invalid AI platform ID" }
                    };
                }

                var result = await _aiPlatformDataContext.DeleteAsync(id, (int)userId.GetHashCode());

                return new DeleteEntityResponse
                {
                    Result = result,
                    Status = HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new DeleteEntityResponse
                {
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<AIPlatformResponse<List<AIPlatform>>> GetAllAsync()
        {
            try
            {
                var platforms = await _aiPlatformDataContext.GetAllAsync();
                return new AIPlatformResponse<List<AIPlatform>>
                {
                    Result = platforms
                };
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

        public async Task<AIPlatformResponse<List<AIPlatform>>> GetAllActiveAsync()
        {
            try
            {
                var platforms = await _aiPlatformDataContext.GetAllActiveAsync();
                return new AIPlatformResponse<List<AIPlatform>>
                {
                    Result = platforms
                };
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

        public async Task<AIPlatformResponse<AIPlatform>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return new AIPlatformResponse<AIPlatform>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Invalid AI platform ID" }
                    };
                }

                var platform = await _aiPlatformDataContext.GetByIdAsync(id);
                return new AIPlatformResponse<AIPlatform>
                {
                    Result = platform
                };
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                return new AIPlatformResponse<AIPlatform>
                {
                    Status = HttpStatusCode.NotFound,
                    Errors = new List<string> { ex.Message }
                };
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

        public async Task<AIPlatformResponse<AIPlatform>> GetBySlugAsync(string slug)
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

                var platform = await _aiPlatformDataContext.GetBySlugAsync(slug);
                if (platform == null)
                {
                    return new AIPlatformResponse<AIPlatform>
                    {
                        Status = HttpStatusCode.NotFound,
                        Errors = new List<string> { $"AI platform with slug '{slug}' not found" }
                    };
                }

                return new AIPlatformResponse<AIPlatform>
                {
                    Result = platform
                };
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

        public async Task<AIPlatformResponse<AIPlatform>> GetActiveBySlugAsync(string slug)
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

                var platform = await _aiPlatformDataContext.GetActiveBySlugAsync(slug);
                if (platform == null)
                {
                    return new AIPlatformResponse<AIPlatform>
                    {
                        Status = HttpStatusCode.NotFound,
                        Errors = new List<string> { $"Active AI platform with slug '{slug}' not found" }
                    };
                }

                return new AIPlatformResponse<AIPlatform>
                {
                    Result = platform
                };
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

        public async Task<AIPlatformResponse<AIPlatform>> GetByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return new AIPlatformResponse<AIPlatform>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Name is required" }
                    };
                }

                var platform = await _aiPlatformDataContext.GetByNameAsync(name);
                if (platform == null)
                {
                    return new AIPlatformResponse<AIPlatform>
                    {
                        Status = HttpStatusCode.NotFound,
                        Errors = new List<string> { $"AI platform with name '{name}' not found" }
                    };
                }

                return new AIPlatformResponse<AIPlatform>
                {
                    Result = platform
                };
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

        public async Task<AIPlatformResponse<AIPlatform>> GetActiveByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return new AIPlatformResponse<AIPlatform>
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Name is required" }
                    };
                }

                var platform = await _aiPlatformDataContext.GetActiveByNameAsync(name);
                if (platform == null)
                {
                    return new AIPlatformResponse<AIPlatform>
                    {
                        Status = HttpStatusCode.NotFound,
                        Errors = new List<string> { $"Active AI platform with name '{name}' not found" }
                    };
                }

                return new AIPlatformResponse<AIPlatform>
                {
                    Result = platform
                };
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

        public async Task<AIPlatformResponse<List<AIPlatform>>> SearchByNameAsync(string searchTerm)
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

                var platforms = await _aiPlatformDataContext.SearchByNameAsync(searchTerm);
                return new AIPlatformResponse<List<AIPlatform>>
                {
                    Result = platforms
                };
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

        public async Task<AIPlatformResponse<List<AIPlatform>>> SearchActiveByNameAsync(string searchTerm)
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

                var platforms = await _aiPlatformDataContext.SearchActiveByNameAsync(searchTerm);
                return new AIPlatformResponse<List<AIPlatform>>
                {
                    Result = platforms
                };
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

        public async Task<AIPlatformResponse<List<AIPlatform>>> GetOrderedByDisplayAsync()
        {
            try
            {
                var platforms = await _aiPlatformDataContext.GetOrderedByDisplayAsync();
                return new AIPlatformResponse<List<AIPlatform>>
                {
                    Result = platforms
                };
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

        public async Task<bool> TestPlatformConnectivityAsync(string platformSlug)
        {
            try
            {
                var platformResponse = await GetActiveBySlugAsync(platformSlug);
                if (platformResponse.Result == null)
                {
                    return false;
                }

                // For now, just return true if platform exists and is active
                // This can be extended with actual connectivity testing when needed
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}