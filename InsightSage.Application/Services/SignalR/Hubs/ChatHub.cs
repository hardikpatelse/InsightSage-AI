using InsightSage.Shared.Interfaces.Others;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using InsightSage.Shared.Exceptions;

namespace InsightSage.Application.Services.SignalR.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IUserContext _userContext;

        public ChatHub(IUserContext userContext)
        {
            _userContext = userContext;
        }

        public override async Task OnConnectedAsync()
        {
            // Add user to their personal group for targeted messages
            var userId = GetCurrentUserId().ToString();
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

            // Optionally add to session-specific groups
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId().ToString();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join a specific chat session group
        /// </summary>
        public async Task JoinSession(string sessionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}");
        }

        /// <summary>
        /// Leave a specific chat session group
        /// </summary>
        public async Task LeaveSession(string sessionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session-{sessionId}");
        }

        /// <summary>
        /// Extract User ID from Hub Context instead of HttpContextAccessor
        /// </summary>
        private Guid GetCurrentUserId()
        {
            try
            {
                var user = Context.User;
                if (user == null || !user.Identity?.IsAuthenticated == true)
                {
                    throw new UnauthorizedException("User is not authenticated in SignalR context.");
                }

                // Try multiple claim types for user ID - same logic as UserContext
                var id = user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value ??
                        user.FindFirst("oid")?.Value ??
                        user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                        user.FindFirst("sub")?.Value;

                if (Guid.TryParse(id, out var guid) && guid != Guid.Empty)
                {
                    return guid;
                }

                // If we can't find a valid GUID, create a detailed error message with available claims
                var availableClaims = string.Join(", ", user.Claims.Select(c => $"{c.Type}={c.Value}"));
                throw new UnauthorizedException($"User ID not found or invalid in SignalR context claims. Available claims: {availableClaims}");
            }
            catch (UnauthorizedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnauthorizedException("Failed to extract user ID from SignalR context.", ex);
            }
        }
    }
}
