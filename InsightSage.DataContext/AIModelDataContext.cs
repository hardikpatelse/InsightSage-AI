using InsightSage.Data;
using InsightSage.Shared.Interfaces.DataContexts;
using InsightSage.Shared.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsightSage.DataContext
{
    public class AIModelDataContext : IAIModelDataContext
    {
        private readonly InsightSageDbContext _context;

        public AIModelDataContext(InsightSageDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(AIModel data)
        {
            try
            {
                // Reset Id to 0 to ensure it's treated as a new entity
                data.Id = 0;
                data.CreatedAt = DateTime.UtcNow;
                data.UpdatedAt = DateTime.UtcNow;

                _context.AIModels.Add(data);
                await _context.SaveChangesAsync();

                return data.Id;
            }
            catch (DbUpdateException ex)
                when (ex.InnerException?.Message?.Contains("IX_AIModels_Name_Unique") == true)
            {
                throw new InvalidOperationException($"An AI model with name '{data.Name}' already exists.", ex);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException?.Message?.Contains("IX_AIModels_Slug_Unique") == true)
            {
                throw new InvalidOperationException($"An AI model with slug '{data.Slug}' already exists.", ex);
            }
        }

        public async Task<string> DeleteAsync(int id, int modifiedBy)
        {
            var model = await _context.AIModels.FindAsync(id);
            if (model != null)
            {
                _context.AIModels.Remove(model);
                await _context.SaveChangesAsync();
                return "AI model deleted successfully";
            }
            return "AI model not found";
        }

        public async Task<List<AIModel>> GetAllAsync()
        {
            return await _context.AIModels
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<List<AIModel>> GetAllActiveAsync()
        {
            return await _context.AIModels
                .Where(m => m.IsActive)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<AIModel> GetByIdAsync(int id)
        {
            var model = await _context.AIModels.FindAsync(id);
            return model ?? throw new InvalidOperationException($"AI model with ID {id} not found");
        }

        public async Task<AIModel?> GetBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return null;

            return await _context.AIModels
                .FirstOrDefaultAsync(m => m.Slug == slug);
        }

        public async Task<AIModel?> GetActiveBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return null;

            return await _context.AIModels
                .FirstOrDefaultAsync(m => m.Slug == slug && m.IsActive);
        }

        public async Task<AIModel?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return await _context.AIModels
                .FirstOrDefaultAsync(m => m.Name == name);
        }

        public async Task<List<AIModel>> SearchByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<AIModel>();

            return await _context.AIModels
                .Where(m => m.Name.Contains(searchTerm) || m.Description.Contains(searchTerm))
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<List<AIModel>> SearchActiveByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<AIModel>();

            return await _context.AIModels
                .Where(m => m.IsActive && (m.Name.Contains(searchTerm) || m.Description.Contains(searchTerm)))
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<int> UpdateAsync(AIModel data)
        {
            try
            {
                data.UpdatedAt = DateTime.UtcNow;

                _context.AIModels.Update(data);
                await _context.SaveChangesAsync();
                return data.Id;
            }
            catch (DbUpdateException ex)
                when (ex.InnerException?.Message?.Contains("IX_AIModels_Name_Unique") == true)
            {
                throw new InvalidOperationException($"An AI model with name '{data.Name}' already exists.", ex);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException?.Message?.Contains("IX_AIModels_Slug_Unique") == true)
            {
                throw new InvalidOperationException($"An AI model with slug '{data.Slug}' already exists.", ex);
            }
        }
    }
}