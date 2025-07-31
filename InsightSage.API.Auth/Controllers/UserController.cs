using InsightSage.Shared.Interfaces.Services;
using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace InsightSage.API.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
                throw;
            }
            return result;
        }
    }
}
