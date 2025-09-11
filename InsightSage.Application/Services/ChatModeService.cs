using InsightSage.Shared.Interfaces.DataContexts;
using InsightSage.Shared.Interfaces.Services;
using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Responses;
using InsightSage.Shared.Models.Responses.Common;
using InsightSage.Shared.Exceptions;
using System.Net;

namespace InsightSage.Application.Services
{
    public class ChatModeService : IChatModeService<ChatModeResponse<List<ChatMode>>, ChatModeResponse<ChatMode>, ChatMode>
    {
        private readonly IChatModeDataContext _chatModeDataContext;

        public ChatModeService(IChatModeDataContext chatModeDataContext)
        {
            _chatModeDataContext = chatModeDataContext;
        }

        public async Task<AddUpdateEntityResponse> AddUpdateAsync(ChatMode data)
        {
            try
            {
                if (data == null)
                {
                    return new AddUpdateEntityResponse
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Chat mode data is required" }
                    };
                }

                if (string.IsNullOrWhiteSpace(data.Name))
                {
                    return new AddUpdateEntityResponse
                    {
                        Status = HttpStatusCode.BadRequest,
                        Errors = new List<string> { "Chat mode name is required" }
                    };
                }

                int id;
                if (data.Id == 0)
                {
                    // Add new chat mode
                    id = await _chatModeDataContext.AddAsync(data);
                }
                else
                {
                    // Update existing chat mode
                    id = await _chatModeDataContext.UpdateAsync(data);
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
                        Errors = new List<string> { "Invalid chat mode ID" }
                    };
                }

                var result = await _chatModeDataContext.DeleteAsync(id, (int)userId.GetHashCode());

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

        public async Task<ChatModeResponse<List<ChatMode>>> GetAllAsync()
        {
            try
            {
                var modes = await _chatModeDataContext.GetAllAsync();
                return new ChatModeResponse<List<ChatMode>>
                {
                    Result = modes,
                    Status = HttpStatusCode.OK
                };
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

        public async Task<ChatModeResponse<List<ChatMode>>> GetAllActiveAsync()
        {
            try
            {
                var modes = await _chatModeDataContext.GetAllActiveAsync();
                return new ChatModeResponse<List<ChatMode>>
                {
                    Result = modes,
                    Status = HttpStatusCode.OK
                };
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

        public async Task<ChatModeResponse<ChatMode>> GetByIdAsync(int id)
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

                var mode = await _chatModeDataContext.GetByIdAsync(id);
                return new ChatModeResponse<ChatMode>
                {
                    Result = mode,
                    Status = HttpStatusCode.OK
                };
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                return new ChatModeResponse<ChatMode>
                {
                    Status = HttpStatusCode.NotFound,
                    Errors = new List<string> { ex.Message }
                };
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

        public async Task<ChatModeResponse<ChatMode>> GetBySlugAsync(string slug)
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

                var mode = await _chatModeDataContext.GetBySlugAsync(slug);
                if (mode == null)
                {
                    return new ChatModeResponse<ChatMode>
                    {
                        Status = HttpStatusCode.NotFound,
                        Errors = new List<string> { $"Chat mode with slug '{slug}' not found" }
                    };
                }

                return new ChatModeResponse<ChatMode>
                {
                    Result = mode,
                    Status = HttpStatusCode.OK
                };
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

        public async Task<ChatModeResponse<ChatMode>> GetActiveBySlugAsync(string slug)
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

                var mode = await _chatModeDataContext.GetActiveBySlugAsync(slug);
                if (mode == null)
                {
                    return new ChatModeResponse<ChatMode>
                    {
                        Status = HttpStatusCode.NotFound,
                        Errors = new List<string> { $"Active chat mode with slug '{slug}' not found" }
                    };
                }

                return new ChatModeResponse<ChatMode>
                {
                    Result = mode,
                    Status = HttpStatusCode.OK
                };
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

        public async Task<ChatModeResponse<ChatMode>> GetByNameAsync(string name)
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

                var mode = await _chatModeDataContext.GetByNameAsync(name);
                if (mode == null)
                {
                    return new ChatModeResponse<ChatMode>
                    {
                        Status = HttpStatusCode.NotFound,
                        Errors = new List<string> { $"Chat mode with name '{name}' not found" }
                    };
                }

                return new ChatModeResponse<ChatMode>
                {
                    Result = mode,
                    Status = HttpStatusCode.OK
                };
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

        public async Task<ChatModeResponse<List<ChatMode>>> SearchByNameAsync(string searchTerm)
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

                var modes = await _chatModeDataContext.SearchByNameAsync(searchTerm);
                return new ChatModeResponse<List<ChatMode>>
                {
                    Result = modes,
                    Status = HttpStatusCode.OK
                };
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

        public async Task<ChatModeResponse<List<ChatMode>>> SearchActiveByNameAsync(string searchTerm)
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

                var modes = await _chatModeDataContext.SearchActiveByNameAsync(searchTerm);
                return new ChatModeResponse<List<ChatMode>>
                {
                    Result = modes,
                    Status = HttpStatusCode.OK
                };
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

        public async Task<ChatModeResponse<List<ChatMode>>> GetAllWithIconsAsync()
        {
            try
            {
                var modes = await _chatModeDataContext.GetAllWithIconsAsync();
                return new ChatModeResponse<List<ChatMode>>
                {
                    Result = modes,
                    Status = HttpStatusCode.OK
                };
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

        public async Task<ChatModeResponse<List<ChatMode>>> GetAllActiveWithIconsAsync()
        {
            try
            {
                var modes = await _chatModeDataContext.GetAllActiveWithIconsAsync();
                return new ChatModeResponse<List<ChatMode>>
                {
                    Result = modes,
                    Status = HttpStatusCode.OK
                };
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
    }
}