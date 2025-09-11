using InsightSage.Shared.Interfaces.Others;
using InsightSage.Shared.Interfaces.Services;
using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InsightSage.API.Chat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthTestController : ControllerBase
    {
        private readonly IUserContext _userContext;

        public AuthTestController(IUserContext userContext)
        {
            _userContext = userContext;
        }
        /// <summary>
        /// Debug endpoint to check authentication status
        /// </summary>
        [HttpGet("debug/auth")]
        public IActionResult AuthenticationDebug()
        {
            try
            {
                return Ok(new
                {
                    message = "Authentication successful for Chat APIs",
                    timestamp = DateTime.UtcNow,
                    userId = _userContext.UserId,
                    userEmail = _userContext.Email,
                    userName = _userContext.Name,
                    claims = HttpContext?.User?.Claims?.ToDictionary(c => c.Type, c => c.Value) ?? new Dictionary<string, string>()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to get authentication debug info",
                    message = ex.Message
                });
            }
        }
    }
}
