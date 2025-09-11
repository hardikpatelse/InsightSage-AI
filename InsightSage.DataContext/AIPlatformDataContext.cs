using InsightSage.Data;
using InsightSage.Shared.Interfaces.DataContexts;
using InsightSage.Shared.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsightSage.DataContext
{
    public class AIPlatformDataContext : IAIPlatformDataContext
    {
        private readonly InsightSageDbContext _context;

        public AIPlatformDataContext(InsightSageDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(AIPlatform data)
        {
            try
            {
                // Reset Id to 0 to ensure it's treated as a new entity
                data.Id = 0;
                data.CreatedAt = DateTime.UtcNow;
                data.UpdatedAt = DateTime.UtcNow;

                _context.AIPlatforms.Add(data);
                await _context.SaveChangesAsync();

                return data.Id;
            }
            catch (DbUpdateException ex)
                when (ex.InnerException?.Message?.Contains("IX_AIPlatforms_Name_Unique") == true)
            {
                throw new InvalidOperationException($"An AI platform with name '{data.Name}' already exists.", ex);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException?.Message?.Contains("IX_AIPlatforms_Slug_Unique") == true)
            {
                throw new InvalidOperationException($"An AI platform with slug '{data.Slug}' already exists.", ex);
            }
        }

        public async Task<string> DeleteAsync(int id, int modifiedBy)
        {
            var platform = await _context.AIPlatforms.FindAsync(id);
            if (platform != null)
            {
                _context.AIPlatforms.Remove(platform);
                await _context.SaveChangesAsync();
                return "AI platform deleted successfully";
            }
            return "AI platform not found";
        }

        public async Task<List<AIPlatform>> GetAllAsync()
        {
            return await _context.AIPlatforms
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<List<AIPlatform>> GetAllActiveAsync()
        {
            return await _context.AIPlatforms
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<AIPlatform> GetByIdAsync(int id)
        {
            var platform = await _context.AIPlatforms.FindAsync(id);
            return platform ?? throw new InvalidOperationException($"AI platform with ID {id} not found");
        }

        public async Task<AIPlatform?> GetBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return null;

            return await _context.AIPlatforms
                .FirstOrDefaultAsync(p => p.Slug == slug);
        }

        public async Task<AIPlatform?> GetActiveBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return null;

            return await _context.AIPlatforms
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);
        }

        public async Task<AIPlatform?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return await _context.AIPlatforms
                .FirstOrDefaultAsync(p => p.Name == name);
        }

        public async Task<AIPlatform?> GetActiveByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return await _context.AIPlatforms
                .FirstOrDefaultAsync(p => p.Name == name && p.IsActive);
        }

        public async Task<List<AIPlatform>> SearchByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<AIPlatform>();

            return await _context.AIPlatforms
                .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<List<AIPlatform>> SearchActiveByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<AIPlatform>();

            return await _context.AIPlatforms
                .Where(p => p.IsActive && (p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm)))
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<List<AIPlatform>> GetOrderedByDisplayAsync()
        {
            return await _context.AIPlatforms
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<int> UpdateAsync(AIPlatform data)
        {
            try
            {
                data.UpdatedAt = DateTime.UtcNow;

                _context.AIPlatforms.Update(data);
                await _context.SaveChangesAsync();
                return data.Id;
            }
            catch (DbUpdateException ex)
                when (ex.InnerException?.Message?.Contains("IX_AIPlatforms_Name_Unique") == true)
            {
                throw new InvalidOperationException($"An AI platform with name '{data.Name}' already exists.", ex);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException?.Message?.Contains("IX_AIPlatforms_Slug_Unique") == true)
            {
                throw new InvalidOperationException($"An AI platform with slug '{data.Slug}' already exists.", ex);
            }
        }
    }
}