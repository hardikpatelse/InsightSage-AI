using System.Security.Claims;
using InsightSage.Shared.Interfaces.Others;
using InsightSage.Shared.Exceptions;

namespace InsightSage.Infrastructure
{
    /// <summary>
    /// SignalR-specific user context that doesn't rely on HttpContextAccessor
    /// </summary>
    public class SignalRUserContext : IUserContext
    {
        private readonly ClaimsPrincipal _user;

        public SignalRUserContext(ClaimsPrincipal user)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        public Guid UserId
        {
            get
            {
                try
                {
                    if (_user == null || !_user.Identity?.IsAuthenticated == true)
                    {
                        throw new UnauthorizedException("User is not authenticated in SignalR context.");
                    }

                    // Try multiple claim types for user ID - including Microsoft's standard claims
                    var id = _user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value ?? 
                            _user.FindFirst("oid")?.Value ?? 
                            _user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                            _user.FindFirst("sub")?.Value; // Microsoft's object identifier claim

                    if (Guid.TryParse(id, out var guid) && guid != Guid.Empty)
                    {
                        return guid;
                    }
                    
                    // If we can't find a valid GUID, create a detailed error message with available claims
                    var availableClaims = string.Join(", ", _user.Claims.Select(c => $"{c.Type}={c.Value}"));
                    throw new UnauthorizedException($"User ID not found or invalid in SignalR token claims. Available claims: {availableClaims}");
                }
                catch (UnauthorizedException)
                {
                    // Re-throw UnauthorizedException as-is
                    throw;
                }
                catch (Exception ex)
                {
                    throw new UnauthorizedException("User is not authenticated or user ID is invalid in SignalR context.", ex);
                }
            }
        }

        public string? Email => _user?.FindFirst("emails")?.Value ?? 
                               _user?.FindFirst(ClaimTypes.Email)?.Value ??
                               _user?.FindFirst("preferred_username")?.Value; // Microsoft often uses preferred_username for email

        public string? Name => _user?.FindFirst("name")?.Value ?? _user?.Identity?.Name;
        
        public string? TenantId => _user?.FindFirst("tid")?.Value ?? 
                                  _user?.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value; // Microsoft's tenant ID claim

        public string? Role => _user?.FindFirst(ClaimTypes.Role)?.Value;
    }
}