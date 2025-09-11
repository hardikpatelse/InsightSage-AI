using InsightSage.Shared.Interfaces.Services.Common;
using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Responses;

namespace InsightSage.Shared.Interfaces.Services
{
    public interface IAIPlatformService<T, U, V> : IEntityService<T, U, V>
    {
        /// <summary>
        /// Get AI Platform by slug
        /// </summary>
        /// <param name="slug">The slug to search for</param>
        /// <returns>AIPlatformResponse with the entity or error information</returns>
        Task<AIPlatformResponse<AIPlatform>> GetBySlugAsync(string slug);

        /// <summary>
        /// Get AI Platform by name
        /// </summary>
        /// <param name="name">The name to search for</param>
        /// <returns>AIPlatformResponse with the entity or error information</returns>
        Task<AIPlatformResponse<AIPlatform>> GetByNameAsync(string name);

        /// <summary>
        /// Search AI Platforms by name pattern
        /// </summary>
        /// <param name="searchTerm">Search term to match against names</param>
        /// <returns>AIPlatformResponse with list of matching entities</returns>
        Task<AIPlatformResponse<List<AIPlatform>>> SearchByNameAsync(string searchTerm);

        /// <summary>
        /// Get all active AI platforms
        /// </summary>
        /// <returns>AIPlatformResponse with list of active entities</returns>
        Task<AIPlatformResponse<List<AIPlatform>>> GetAllActiveAsync();

        /// <summary>
        /// Get active AI Platform by slug
        /// </summary>
        /// <param name="slug">The slug to search for</param>
        /// <returns>AIPlatformResponse with the active entity or error information</returns>
        Task<AIPlatformResponse<AIPlatform>> GetActiveBySlugAsync(string slug);

        /// <summary>
        /// Get active AI Platform by name
        /// </summary>
        /// <param name="name">The name to search for</param>
        /// <returns>AIPlatformResponse with the active entity or error information</returns>
        Task<AIPlatformResponse<AIPlatform>> GetActiveByNameAsync(string name);

        /// <summary>
        /// Search active AI Platforms by name pattern
        /// </summary>
        /// <param name="searchTerm">Search term to match against names</param>
        /// <returns>AIPlatformResponse with list of matching active entities</returns>
        Task<AIPlatformResponse<List<AIPlatform>>> SearchActiveByNameAsync(string searchTerm);

        /// <summary>
        /// Get AI Platforms ordered by display order
        /// </summary>
        /// <returns>AIPlatformResponse with list of entities ordered by display order</returns>
        Task<AIPlatformResponse<List<AIPlatform>>> GetOrderedByDisplayAsync();

        /// <summary>
        /// Test connectivity to a specific platform
        /// </summary>
        /// <param name="platformSlug">Platform slug to test</param>
        /// <returns>True if platform is accessible, false otherwise</returns>
        Task<bool> TestPlatformConnectivityAsync(string platformSlug);
    }
}