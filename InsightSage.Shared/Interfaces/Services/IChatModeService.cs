using InsightSage.Shared.Interfaces.Services.Common;
using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Responses;

namespace InsightSage.Shared.Interfaces.Services
{
    public interface IChatModeService<T, U, V> : IEntityService<T, U, V>
    {
        /// <summary>
        /// Get ChatMode by slug
        /// </summary>
        /// <param name="slug">The slug to search for</param>
        /// <returns>ChatModeResponse with the entity or error information</returns>
        Task<ChatModeResponse<ChatMode>> GetBySlugAsync(string slug);

        /// <summary>
        /// Get ChatMode by name
        /// </summary>
        /// <param name="name">The name to search for</param>
        /// <returns>ChatModeResponse with the entity or error information</returns>
        Task<ChatModeResponse<ChatMode>> GetByNameAsync(string name);

        /// <summary>
        /// Search ChatModes by name pattern
        /// </summary>
        /// <param name="searchTerm">Search term to match against names</param>
        /// <returns>ChatModeResponse with list of matching entities</returns>
        Task<ChatModeResponse<List<ChatMode>>> SearchByNameAsync(string searchTerm);

        /// <summary>
        /// Get all ChatModes with icons
        /// </summary>
        /// <returns>ChatModeResponse with list of entities that have icon names</returns>
        Task<ChatModeResponse<List<ChatMode>>> GetAllWithIconsAsync();

        /// <summary>
        /// Get all active chat modes
        /// </summary>
        /// <returns>ChatModeResponse with list of active entities</returns>
        Task<ChatModeResponse<List<ChatMode>>> GetAllActiveAsync();

        /// <summary>
        /// Get active ChatMode by slug
        /// </summary>
        /// <param name="slug">The slug to search for</param>
        /// <returns>ChatModeResponse with the active entity or error information</returns>
        Task<ChatModeResponse<ChatMode>> GetActiveBySlugAsync(string slug);

        /// <summary>
        /// Search active ChatModes by name pattern
        /// </summary>
        /// <param name="searchTerm">Search term to match against names</param>
        /// <returns>ChatModeResponse with list of matching active entities</returns>
        Task<ChatModeResponse<List<ChatMode>>> SearchActiveByNameAsync(string searchTerm);

        /// <summary>
        /// Get all active ChatModes with icons
        /// </summary>
        /// <returns>ChatModeResponse with list of active entities that have icon names</returns>
        Task<ChatModeResponse<List<ChatMode>>> GetAllActiveWithIconsAsync();
    }
}