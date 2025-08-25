using InsightSage.Shared.Interfaces.DataContexts.Common;
using InsightSage.Shared.Models.Entities;

namespace InsightSage.Shared.Interfaces.DataContexts
{
    public interface IChatModeDataContext : IEntityDataContext<ChatMode>
    {
        /// <summary>
        /// Get ChatMode by slug
        /// </summary>
        /// <param name="slug">The slug to search for</param>
        /// <returns>ChatMode entity or null if not found</returns>
        Task<ChatMode?> GetBySlugAsync(string slug);

        /// <summary>
        /// Get ChatMode by name
        /// </summary>
        /// <param name="name">The name to search for</param>
        /// <returns>ChatMode entity or null if not found</returns>
        Task<ChatMode?> GetByNameAsync(string name);

        /// <summary>
        /// Search ChatModes by name pattern
        /// </summary>
        /// <param name="searchTerm">Search term to match against names</param>
        /// <returns>List of matching ChatModes</returns>
        Task<List<ChatMode>> SearchByNameAsync(string searchTerm);

        /// <summary>
        /// Get all ChatModes with icons
        /// </summary>
        /// <returns>List of ChatModes that have icon names</returns>
        Task<List<ChatMode>> GetAllWithIconsAsync();

        /// <summary>
        /// Get all active chat modes
        /// </summary>
        /// <returns>List of active ChatModes</returns>
        Task<List<ChatMode>> GetAllActiveAsync();

        /// <summary>
        /// Get active ChatMode by slug
        /// </summary>
        /// <param name="slug">The slug to search for</param>
        /// <returns>Active ChatMode entity or null if not found</returns>
        Task<ChatMode?> GetActiveBySlugAsync(string slug);

        /// <summary>
        /// Search active ChatModes by name pattern
        /// </summary>
        /// <param name="searchTerm">Search term to match against names</param>
        /// <returns>List of matching active ChatModes</returns>
        Task<List<ChatMode>> SearchActiveByNameAsync(string searchTerm);

        /// <summary>
        /// Get all active ChatModes with icons
        /// </summary>
        /// <returns>List of active ChatModes that have icon names</returns>
        Task<List<ChatMode>> GetAllActiveWithIconsAsync();
    }
}