using InsightSage.Shared.Interfaces.Services;
using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace InsightSage.API.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Comment this out for testing
    public class UserController : ControllerBase
    {
        private readonly IUserService<UserResponse<List<User>>, UserResponse<User>, User> _userService;

        public UserController(IUserService<UserResponse<List<User>>, UserResponse<User>, User> userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<UserResponse<User>> GetUser()
        {
            UserResponse<User> result = new();
            try
            {
                result = await _userService.GetCurrentUser();
                result.Status = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                result.Status = HttpStatusCode.InternalServerError;
                result.Errors.Add(ex.GetBaseException().Message);
                result.ExceptionDetails = ex.ToString();
            }
            return result;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<UserResponse<User>> Login([FromBody] User user)
        {
            UserResponse<User> result = new();
            try
            {
                // Fix 1: Validate user input and return early if invalid
                if (user == null)
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("User data is required");
                    return result; // Return early to prevent further processing
                }

                if (string.IsNullOrEmpty(user.Email))
                {
                    result.Status = HttpStatusCode.BadRequest;
                    result.Errors.Add("Email is required");
                    return result; // Return early to prevent service call
                }

                // Only call service if validation passes
                result = await _userService.LoginAsync(user);
                
                // Fix 2: Don't override status if there are errors
                if (result.Errors.Any())
                {
                    result.Status = HttpStatusCode.InternalServerError;
                }
                else
                {
                    result.Status = HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                result.Status = HttpStatusCode.InternalServerError;
                result.Errors.Add(ex.GetBaseException().Message);
                result.ExceptionDetails = ex.ToString();
            }
            return result;
        }

        [HttpGet("health")]
        [AllowAnonymous] // Health check endpoint for testing
        public IActionResult HealthCheck()
        {
            return Ok(new { 
                status = "healthy", 
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            });
        }
    }
}
