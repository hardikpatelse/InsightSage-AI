using InsightSage.Shared.Interfaces.DataContexts.Common;
using InsightSage.Shared.Models.Entities;

namespace InsightSage.Shared.Interfaces.DataContexts
{
    public interface IAIPlatformDataContext : IEntityDataContext<AIPlatform>
    {
        /// <summary>
        /// Get AI Platform by slug
        /// </summary>
        /// <param name="slug">The slug to search for</param>
        /// <returns>AIPlatform entity or null if not found</returns>
        Task<AIPlatform?> GetBySlugAsync(string slug);

        /// <summary>
        /// Get AI Platform by name
        /// </summary>
        /// <param name="name">The name to search for</param>
        /// <returns>AIPlatform entity or null if not found</returns>
        Task<AIPlatform?> GetByNameAsync(string name);

        /// <summary>
        /// Search AI Platforms by name pattern
        /// </summary>
        /// <param name="searchTerm">Search term to match against names</param>
        /// <returns>List of matching AI Platforms</returns>
        Task<List<AIPlatform>> SearchByNameAsync(string searchTerm);

        /// <summary>
        /// Get all active AI platforms ordered by display order
        /// </summary>
        /// <returns>List of active AI Platforms</returns>
        Task<List<AIPlatform>> GetAllActiveAsync();

        /// <summary>
        /// Get active AI Platform by slug
        /// </summary>
        /// <param name="slug">The slug to search for</param>
        /// <returns>Active AI Platform entity or null if not found</returns>
        Task<AIPlatform?> GetActiveBySlugAsync(string slug);

        /// <summary>
        /// Get active AI Platform by name
        /// </summary>
        /// <param name="name">The name to search for</param>
        /// <returns>Active AI Platform entity or null if not found</returns>
        Task<AIPlatform?> GetActiveByNameAsync(string name);

        /// <summary>
        /// Search active AI Platforms by name pattern
        /// </summary>
        /// <param name="searchTerm">Search term to match against names</param>
        /// <returns>List of matching active AI Platforms</returns>
        Task<List<AIPlatform>> SearchActiveByNameAsync(string searchTerm);

        /// <summary>
        /// Get AI Platforms ordered by display order
        /// </summary>
        /// <returns>List of AI Platforms ordered by display order</returns>
        Task<List<AIPlatform>> GetOrderedByDisplayAsync();
    }
}