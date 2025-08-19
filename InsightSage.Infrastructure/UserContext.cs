using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using InsightSage.Shared.Interfaces.Others;
using InsightSage.Shared.Exceptions;

namespace InsightSage.Infrastructure
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                try
                {
                    var httpContext = _httpContextAccessor.HttpContext;
                    if (httpContext == null)
                    {
                        throw new UnauthorizedException("HttpContext is not available. User is not authenticated or user ID is invalid. Please log in again to continue.");
                    }

                    var user = httpContext.User;
                    if (user == null || !user.Identity?.IsAuthenticated == true)
                    {
                        throw new UnauthorizedException("User is not authenticated or user ID is invalid. Please log in again to continue.");
                    }

                    // Try multiple claim types for user ID - including Microsoft's standard claims
                    var id = user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value ?? 
                            user.FindFirst("oid")?.Value ?? 
                            user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                            user.FindFirst("sub")?.Value; // Microsoft's object identifier claim

                    if (Guid.TryParse(id, out var guid) && guid != Guid.Empty)
                    {
                        return guid;
                    }
                    
                    // If we can't find a valid GUID, create a detailed error message with available claims
                    var availableClaims = string.Join(", ", user.Claims.Select(c => $"{c.Type}={c.Value}"));
                    throw new UnauthorizedException($"User ID not found or invalid in token claims. Available claims: {availableClaims}");
                }
                catch (UnauthorizedException)
                {
                    // Re-throw UnauthorizedException as-is
                    throw;
                }
                catch (Exception ex)
                {
                    throw new UnauthorizedException("User is not authenticated or user ID is invalid. Please log in again to continue.", ex);
                }
            }
        }

        public string? Email => User?.FindFirst("emails")?.Value ?? 
                               User?.FindFirst(ClaimTypes.Email)?.Value ??
                               User?.FindFirst("preferred_username")?.Value; // Microsoft often uses preferred_username for email

        public string? Name => User?.FindFirst("name")?.Value ?? User?.Identity?.Name;
        
        public string? TenantId => User?.FindFirst("tid")?.Value ?? 
                                  User?.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value; // Microsoft's tenant ID claim

        public string? Role => User?.FindFirst(ClaimTypes.Role)?.Value;
    }
}
