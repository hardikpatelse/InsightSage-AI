using InsightSage.Shared.Interfaces.DataContexts;
using InsightSage.Shared.Interfaces.Services;
using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Responses;
using InsightSage.Shared.Models.Responses.Common;
using InsightSage.Shared.Exceptions;
using System.Net;

namespace InsightSage.Application.Services
{
    public class AIModelService : IAIModelService<AIModelResponse<List<AIModel>>, AIModelResponse<AIModel>, AIModel>
    {
        private readonly IAIModelDataContext _aiModelDataContext;

        public AIModelService(IAIModelDataContext aiModelDataContext)
        {
            _aiModelDataContext = aiModelDataContext;
        }

        public async Task<AddUpdateEntityResponse> AddUpdateAsync(AIModel data)
        {
            try
            {
                if (data == null)
                {
                    return new AddUpdateEntityResponse
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "AI model data is required" }
                    };
                }

                if (string.IsNullOrWhiteSpace(data.Name))
                {
                    return new AddUpdateEntityResponse
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "AI model name is required" }
                    };
                }

                int id;
                if (data.Id == 0)
                {
                    // Add new AI model
                    id = await _aiModelDataContext.AddAsync(data);
                }
                else
                {
                    // Update existing AI model
                    id = await _aiModelDataContext.UpdateAsync(data);
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
                        Errors = new List<string> { "Invalid AI model ID" }
                    };
                }

                var result = await _aiModelDataContext.DeleteAsync(id, (int)userId.GetHashCode());

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

        public async Task<AIModelResponse<List<AIModel>>> GetAllAsync()
        {
            try
            {
                var models = await _aiModelDataContext.GetAllAsync();
                return new AIModelResponse<List<AIModel>>
                {
                    Result = models,
                    Status = HttpStatusCode.OK
                };
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

        public async Task<AIModelResponse<List<AIModel>>> GetAllActiveAsync()
        {
            try
            {
                var models = await _aiModelDataContext.GetAllActiveAsync();
                return new AIModelResponse<List<AIModel>>
                {
                    Result = models,
                    Status = HttpStatusCode.OK
                };
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

        public async Task<AIModelResponse<AIModel>> GetByIdAsync(int id)
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

                var model = await _aiModelDataContext.GetByIdAsync(id);
                return new AIModelResponse<AIModel>
                {
                    Result = model,
                    Status = HttpStatusCode.OK
                };
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                return new AIModelResponse<AIModel>
                {
                    Status = HttpStatusCode.NotFound,
                    Errors = new List<string> { ex.Message }
                };
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

        public async Task<AIModelResponse<AIModel>> GetBySlugAsync(string slug)
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

                var model = await _aiModelDataContext.GetBySlugAsync(slug);
                if (model == null)
                {
                    return new AIModelResponse<AIModel>
                    {
                        Status = HttpStatusCode.NotFound,
                        Errors = new List<string> { $"AI model with slug '{slug}' not found" }
                    };
                }

                return new AIModelResponse<AIModel>
                {
                    Result = model,
                    Status = HttpStatusCode.OK
                };
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

        public async Task<AIModelResponse<AIModel>> GetActiveBySlugAsync(string slug)
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

                var model = await _aiModelDataContext.GetActiveBySlugAsync(slug);
                if (model == null)
                {
                    return new AIModelResponse<AIModel>
                    {
                        Status = HttpStatusCode.NotFound,
                        Errors = new List<string> { $"Active AI model with slug '{slug}' not found" }
                    };
                }

                return new AIModelResponse<AIModel>
                {
                    Result = model,
                    Status = HttpStatusCode.OK
                };
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

        public async Task<AIModelResponse<AIModel>> GetByNameAsync(string name)
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

                var model = await _aiModelDataContext.GetByNameAsync(name);
                if (model == null)
                {
                    return new AIModelResponse<AIModel>
                    {
                        Status = HttpStatusCode.NotFound,
                        Errors = new List<string> { $"AI model with name '{name}' not found" }
                    };
                }

                return new AIModelResponse<AIModel>
                {
                    Result = model,
                    Status = HttpStatusCode.OK
                };
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

        public async Task<AIModelResponse<List<AIModel>>> SearchByNameAsync(string searchTerm)
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

                var models = await _aiModelDataContext.SearchByNameAsync(searchTerm);
                return new AIModelResponse<List<AIModel>>
                {
                    Result = models,
                    Status = HttpStatusCode.OK
                };
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

        public async Task<AIModelResponse<List<AIModel>>> SearchActiveByNameAsync(string searchTerm)
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

                var models = await _aiModelDataContext.SearchActiveByNameAsync(searchTerm);
                return new AIModelResponse<List<AIModel>>
                {
                    Result = models,
                    Status = HttpStatusCode.OK
                };
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
    }
}