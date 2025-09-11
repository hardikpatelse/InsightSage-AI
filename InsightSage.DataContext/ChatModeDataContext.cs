using InsightSage.Data;
using InsightSage.Shared.Interfaces.DataContexts;
using InsightSage.Shared.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsightSage.DataContext
{
    public class ChatModeDataContext : IChatModeDataContext
    {
        private readonly InsightSageDbContext _context;

        public ChatModeDataContext(InsightSageDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(ChatMode data)
        {
            try
            {
                // Reset Id to 0 to ensure it's treated as a new entity
                data.Id = 0;
                data.CreatedAt = DateTime.UtcNow;
                data.UpdatedAt = DateTime.UtcNow;

                _context.ChatModes.Add(data);
                await _context.SaveChangesAsync();

                return data.Id;
            }
            catch (DbUpdateException ex)
                when (ex.InnerException?.Message?.Contains("IX_ChatModes_Name_Unique") == true)
            {
                throw new InvalidOperationException($"A chat mode with name '{data.Name}' already exists.", ex);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException?.Message?.Contains("IX_ChatModes_Slug_Unique") == true)
            {
                throw new InvalidOperationException($"A chat mode with slug '{data.Slug}' already exists.", ex);
            }
        }

        public async Task<string> DeleteAsync(int id, int modifiedBy)
        {
            var mode = await _context.ChatModes.FindAsync(id);
            if (mode != null)
            {
                _context.ChatModes.Remove(mode);
                await _context.SaveChangesAsync();
                return "Chat mode deleted successfully";
            }
            return "Chat mode not found";
        }

        public async Task<List<ChatMode>> GetAllAsync()
        {
            return await _context.ChatModes
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<List<ChatMode>> GetAllActiveAsync()
        {
            return await _context.ChatModes
                .Where(m => m.IsActive)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<ChatMode> GetByIdAsync(int id)
        {
            var mode = await _context.ChatModes.FindAsync(id);
            return mode ?? throw new InvalidOperationException($"Chat mode with ID {id} not found");
        }

        public async Task<ChatMode?> GetBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return null;

            return await _context.ChatModes
                .FirstOrDefaultAsync(m => m.Slug == slug);
        }

        public async Task<ChatMode?> GetActiveBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return null;

            return await _context.ChatModes
                .FirstOrDefaultAsync(m => m.Slug == slug && m.IsActive);
        }

        public async Task<ChatMode?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return await _context.ChatModes
                .FirstOrDefaultAsync(m => m.Name == name);
        }

        public async Task<List<ChatMode>> SearchByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<ChatMode>();

            return await _context.ChatModes
                .Where(m => m.Name.Contains(searchTerm) || m.Description.Contains(searchTerm))
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<List<ChatMode>> SearchActiveByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<ChatMode>();

            return await _context.ChatModes
                .Where(m => m.IsActive && (m.Name.Contains(searchTerm) || m.Description.Contains(searchTerm)))
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<List<ChatMode>> GetAllWithIconsAsync()
        {
            return await _context.ChatModes
                .Where(m => !string.IsNullOrEmpty(m.IconName))
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<List<ChatMode>> GetAllActiveWithIconsAsync()
        {
            return await _context.ChatModes
                .Where(m => m.IsActive && !string.IsNullOrEmpty(m.IconName))
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<int> UpdateAsync(ChatMode data)
        {
            try
            {
                data.UpdatedAt = DateTime.UtcNow;

                _context.ChatModes.Update(data);
                await _context.SaveChangesAsync();
                return data.Id;
            }
            catch (DbUpdateException ex)
                when (ex.InnerException?.Message?.Contains("IX_ChatModes_Name_Unique") == true)
            {
                throw new InvalidOperationException($"A chat mode with name '{data.Name}' already exists.", ex);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException?.Message?.Contains("IX_ChatModes_Slug_Unique") == true)
            {
                throw new InvalidOperationException($"A chat mode with slug '{data.Slug}' already exists.", ex);
            }
        }
    }
}