using InsightSage.Shared.Interfaces.Services.Common;
using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Responses;

namespace InsightSage.Shared.Interfaces.Services
{
    public interface IAIModelService<T, U, V> : IEntityService<T, U, V>
    {
        /// <summary>
        /// Get AIModel by slug
        /// </summary>
        /// <param name="slug">The slug to search for</param>
        /// <returns>AIModelResponse with the entity or error information</returns>
        Task<AIModelResponse<AIModel>> GetBySlugAsync(string slug);

        /// <summary>
        /// Get AIModel by name
        /// </summary>
        /// <param name="name">The name to search for</param>
        /// <returns>AIModelResponse with the entity or error information</returns>
        Task<AIModelResponse<AIModel>> GetByNameAsync(string name);

        /// <summary>
        /// Search AIModels by name pattern
        /// </summary>
        /// <param name="searchTerm">Search term to match against names</param>
        /// <returns>AIModelResponse with list of matching entities</returns>
        Task<AIModelResponse<List<AIModel>>> SearchByNameAsync(string searchTerm);

        /// <summary>
        /// Get all active AI models
        /// </summary>
        /// <returns>AIModelResponse with list of active entities</returns>
        Task<AIModelResponse<List<AIModel>>> GetAllActiveAsync();

        /// <summary>
        /// Get active AIModel by slug
        /// </summary>
        /// <param name="slug">The slug to search for</param>
        /// <returns>AIModelResponse with the active entity or error information</returns>
        Task<AIModelResponse<AIModel>> GetActiveBySlugAsync(string slug);

        /// <summary>
        /// Search active AIModels by name pattern
        /// </summary>
        /// <param name="searchTerm">Search term to match against names</param>
        /// <returns>AIModelResponse with list of matching active entities</returns>
        Task<AIModelResponse<List<AIModel>>> SearchActiveByNameAsync(string searchTerm);
    }
}