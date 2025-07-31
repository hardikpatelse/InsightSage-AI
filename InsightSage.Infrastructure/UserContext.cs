using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using InsightSage.Shared.Interfaces.Others;

namespace InsightSage.Application.Services
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId => User?.FindFirst("oid")?.Value ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        public string? Email => User?.FindFirst("emails")?.Value ?? User?.FindFirst(ClaimTypes.Email)?.Value;
        public string? Name => User?.FindFirst("name")?.Value ?? User?.Identity?.Name;
        public string? TenantId => User?.FindFirst("tid")?.Value;
    }
}
