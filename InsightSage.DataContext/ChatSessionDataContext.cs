using InsightSage.Data;
using InsightSage.Shared.Interfaces.DataContexts;
using InsightSage.Shared.Interfaces.DataContexts.Common;
using InsightSage.Shared.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsightSage.DataContext
{
    public class ChatSessionDataContext : IChatSessionDataContext
    {
        private readonly InsightSageDbContext _context;

        public ChatSessionDataContext(InsightSageDbContext context)
        {
            _context = context;
        }

        async Task<int> IEntityDataContext<ChatSession>.AddAsync(ChatSession data)
        {
            try
            {
                // Reset Id to 0 to ensure it's treated as a new entity
                data.Id = 0;
                data.CreatedAt = DateTime.UtcNow;
                data.UpdatedAt = DateTime.UtcNow;

                // Generate unique identity if not provided
                if (data.UniqueIdentity == Guid.Empty)
                {
                    data.UniqueIdentity = Guid.NewGuid();
                }

                _context.ChatSessions.Add(data);
                await _context.SaveChangesAsync();

                // Return the generated Id
                return data.Id;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                when (ex.InnerException?.Message?.Contains("IX_ChatSessions_UniqueIdentity") == true)
            {
                throw new InvalidOperationException($"A chat session with unique identity '{data.UniqueIdentity}' already exists.", ex);
            }
        }

        async Task<string> IEntityDataContext<ChatSession>.DeleteAsync(int id, int modifiedBy)
        {
            var session = await _context.ChatSessions.FindAsync(id);
            if (session != null)
            {
                _context.ChatSessions.Remove(session);
                await _context.SaveChangesAsync();
                return "Chat session deleted successfully";
            }
            return "Chat session not found";
        }

        async Task<List<ChatSession>> IEntityDataContext<ChatSession>.GetAllAsync()
        {
            return await _context.ChatSessions
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();
        }

        async Task<List<ChatSession>> IChatSessionDataContext.GetAllByUserIdAsync(Guid userId)
        {
            return await _context.ChatSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();
        }

        async Task<ChatSession> IEntityDataContext<ChatSession>.GetByIdAsync(int id)
        {
            var session = await _context.ChatSessions.FindAsync(id);
            return session ?? throw new InvalidOperationException($"Chat session with ID {id} not found");
        }

        async Task<ChatSession?> IChatSessionDataContext.GetByUniqueIdentityAsync(Guid uniqueIdentity)
        {
            return await _context.ChatSessions
                .Include(s => s.AIModel)
                .Include(s => s.ChatMode)
                .FirstOrDefaultAsync(s => s.UniqueIdentity == uniqueIdentity);
        }

        async Task<List<ChatSession>> IChatSessionDataContext.GetSessionsByUserAndNameAsync(Guid userId, string name)
        {
            return await _context.ChatSessions
                .Where(s => s.UserId == userId && s.Name.Contains(name))
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();
        }

        async Task<ChatSession?> IChatSessionDataContext.GetByUserIdAndExactNameAsync(Guid userId, string name)
        {
            return await _context.ChatSessions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Name == name);
        }

        async Task<int> IEntityDataContext<ChatSession>.UpdateAsync(ChatSession data)
        {
            data.UpdatedAt = DateTime.UtcNow;
            
            _context.ChatSessions.Update(data);
            await _context.SaveChangesAsync();
            return data.Id;
        }
    }
}