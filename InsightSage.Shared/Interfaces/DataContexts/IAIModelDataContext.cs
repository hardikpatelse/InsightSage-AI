using InsightSage.Shared.Interfaces.DataContexts.Common;
using InsightSage.Shared.Models.Entities;

namespace InsightSage.Shared.Interfaces.DataContexts
{
    public interface IAIModelDataContext : IEntityDataContext<AIModel>
    {
        /// <summary>
        /// Get AIModel by slug
        /// </summary>
        /// <param name="slug">The slug to search for</param>
        /// <returns>AIModel entity or null if not found</returns>
        Task<AIModel?> GetBySlugAsync(string slug);

        /// <summary>
        /// Get AIModel by name
        /// </summary>
        /// <param name="name">The name to search for</param>
        /// <returns>AIModel entity or null if not found</returns>
        Task<AIModel?> GetByNameAsync(string name);

        /// <summary>
        /// Search AIModels by name pattern
        /// </summary>
        /// <param name="searchTerm">Search term to match against names</param>
        /// <returns>List of matching AIModels</returns>
        Task<List<AIModel>> SearchByNameAsync(string searchTerm);

        /// <summary>
        /// Get all active AI models
        /// </summary>
        /// <returns>List of active AIModels</returns>
        Task<List<AIModel>> GetAllActiveAsync();

        /// <summary>
        /// Get active AIModel by slug
        /// </summary>
        /// <param name="slug">The slug to search for</param>
        /// <returns>Active AIModel entity or null if not found</returns>
        Task<AIModel?> GetActiveBySlugAsync(string slug);

        /// <summary>
        /// Search active AIModels by name pattern
        /// </summary>
        /// <param name="searchTerm">Search term to match against names</param>
        /// <returns>List of matching active AIModels</returns>
        Task<List<AIModel>> SearchActiveByNameAsync(string searchTerm);
    }
}