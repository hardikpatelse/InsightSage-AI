using InsightSage.Data;
using InsightSage.Shared.Interfaces.DataContexts;
using InsightSage.Shared.Interfaces.DataContexts.Common;
using InsightSage.Shared.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsightSage.DataContext
{
    public class UserDataContext : IUserDataContext
    {
        private readonly InsightSageDbContext _context;

        public UserDataContext(InsightSageDbContext context)
        {
            _context = context;
        }

        async Task<int> IEntityDataContext<User>.AddAsync(User data)
        {
            try
            {
                // Reset Id to 0 to ensure it's treated as a new entity
                data.Id = 0;
                data.CreatedAt = DateTime.UtcNow;
                data.UpdatedAt = DateTime.UtcNow;
                
                _context.Users.Add(data);
                await _context.SaveChangesAsync();
                
                // Return the generated Id
                return data.Id;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) 
                when (ex.InnerException?.Message?.Contains("IX_Users_Email_Unique") == true)
            {
                throw new InvalidOperationException($"A user with email '{data.Email}' already exists.", ex);
            }
        }

        async Task<string> IEntityDataContext<User>.DeleteAsync(int id, int modifiedBy)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return "User deleted successfully";
            }
            return "User not found";
        }

        async Task<List<User>> IEntityDataContext<User>.GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        async Task<List<User>> IUserDataContext.GetAllByTenantIdAsync(string tenantId)
        {
            return await _context.Users
                .Where(u => u.TenantId == tenantId)
                .ToListAsync();
        }

        async Task<User?> IUserDataContext.GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        async Task<User> IEntityDataContext<User>.GetByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return user ?? throw new InvalidOperationException($"User with ID {id} not found");
        }

        async Task<User?> IUserDataContext.GetByUserIdAsync(string userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        async Task<int> IEntityDataContext<User>.UpdateAsync(User data)
        {
            data.UpdatedAt = DateTime.UtcNow;
            
            _context.Users.Update(data);
            await _context.SaveChangesAsync();
            return data.Id;
        }
    }
}
