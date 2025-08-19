using InsightSage.Data;
using InsightSage.Shared.Interfaces.DataContexts;
using InsightSage.Shared.Interfaces.DataContexts.Common;
using InsightSage.Shared.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsightSage.DataContext
{
    public class ChatMessageDataContext : IChatMessageDataContext
    {
        private readonly InsightSageDbContext _context;

        public ChatMessageDataContext(InsightSageDbContext context)
        {
            _context = context;
        }

        async Task<int> IEntityDataContext<ChatMessage>.AddAsync(ChatMessage data)
        {
            try
            {
                // Reset Id to 0 to ensure it's treated as a new entity
                data.Id = 0;
                data.CreatedAt = DateTime.UtcNow;
                data.UpdatedAt = DateTime.UtcNow;

                _context.ChatMessages.Add(data);
                await _context.SaveChangesAsync();

                // Return the generated Id
                return data.Id;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error adding chat message: {ex.Message}", ex);
            }
        }

        async Task<string> IEntityDataContext<ChatMessage>.DeleteAsync(int id, int modifiedBy)
        {
            var message = await _context.ChatMessages.FindAsync(id);
            if (message != null)
            {
                _context.ChatMessages.Remove(message);
                await _context.SaveChangesAsync();
                return "Chat message deleted successfully";
            }
            return "Chat message not found";
        }

        async Task<List<ChatMessage>> IEntityDataContext<ChatMessage>.GetAllAsync()
        {
            return await _context.ChatMessages
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        async Task<ChatMessage> IEntityDataContext<ChatMessage>.GetByIdAsync(int id)
        {
            var message = await _context.ChatMessages.FindAsync(id);
            return message ?? throw new InvalidOperationException($"Chat message with ID {id} not found");
        }

        async Task<List<ChatMessage>> IChatMessageDataContext.GetBySessionIdAsync(int sessionId)
        {
            return await _context.ChatMessages
                .Where(m => m.ChatSessionId == sessionId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        async Task<int> IEntityDataContext<ChatMessage>.UpdateAsync(ChatMessage data)
        {
            data.UpdatedAt = DateTime.UtcNow;

            _context.ChatMessages.Update(data);
            await _context.SaveChangesAsync();
            return data.Id;
        }
    }
}