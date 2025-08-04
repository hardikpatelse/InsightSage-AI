using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsightSage.API.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthTestController : ControllerBase
    {
        [HttpGet("anonymous")]
        [AllowAnonymous]
        public IActionResult AnonymousEndpoint()
        {
            return Ok(new { 
                message = "This endpoint works without authentication",
                timestamp = DateTime.UtcNow 
            });
        }

        [HttpGet("protected")]
        [Authorize]
        public IActionResult ProtectedEndpoint()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            
            return Ok(new { 
                message = "Authentication successful!",
                user = User.Identity?.Name ?? "Unknown",
                isAuthenticated = User.Identity?.IsAuthenticated ?? false,
                authType = User.Identity?.AuthenticationType,
                claims = claims,
                timestamp = DateTime.UtcNow 
            });
        }

        [HttpGet("token-info")]
        [Authorize]
        public IActionResult TokenInfo()
        {
            var token = HttpContext.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();
                
            return Ok(new {
                hasToken = !string.IsNullOrEmpty(token),
                tokenLength = token?.Length ?? 0,
                tokenStart = token?.Substring(0, Math.Min(20, token?.Length ?? 0)) + "...",
                userId = User.FindFirst("oid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                email = User.FindFirst("emails")?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value,
                name = User.FindFirst("name")?.Value ?? User.Identity?.Name,
                tenantId = User.FindFirst("tid")?.Value,
                allClaims = User.Claims.ToDictionary(c => c.Type, c => c.Value)
            });
        }
    }
}