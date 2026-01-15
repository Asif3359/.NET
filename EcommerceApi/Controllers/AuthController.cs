using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.DTOs;
using EcommerceApi.Enums;
using EcommerceApi.Helpers;
using EcommerceApi.Interfaces;

namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetUsers()
        {
            var users = await _authService.GetAllUsersAsync();
            
            var userDtos = users.Select(u => new UserInfoDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role.ToString()
            });

            return Ok(ApiResponse<IEnumerable<UserInfoDto>>.SuccessResponse(userDtos, "Users retrieved successfully"));
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult> GetCurrentUser()
        {
            var userId = GetCurrentUserId();
            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return Unauthorized(ApiResponse.FailureResponse("User not found"));
            }

            var userInfo = new UserInfoDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString()
            };

            return Ok(ApiResponse<UserInfoDto>.SuccessResponse(userInfo, "User retrieved successfully"));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult> GetUser(long id)
        {
            var currentUserId = GetCurrentUserId();
            var currentUser = await _authService.GetUserByIdAsync(currentUserId);

            if (currentUser?.Role != UserRole.Admin && currentUserId != id)
            {
                return Forbid();
            }

            var user = await _authService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse.FailureResponse("User not found"));
            }

            var userInfo = new UserInfoDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString()
            };

            return Ok(ApiResponse<UserInfoDto>.SuccessResponse(userInfo, "User retrieved successfully"));
        }

        [HttpPost("signup")]
        public async Task<ActionResult> Signup([FromBody] SignupDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.FailureResponse("Validation failed", errors));
            }

            var (success, message, user, token) = await _authService.SignupAsync(dto);

            if (!success)
            {
                return BadRequest(ApiResponse.FailureResponse(message));
            }

            var response = new
            {
                User = user,
                Token = token
            };

            return Ok(ApiResponse<object>.SuccessResponse(response, message));
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.FailureResponse("Validation failed", errors));
            }

            var (success, message, user, token) = await _authService.LoginAsync(dto);

            if (!success)
            {
                return Unauthorized(ApiResponse.FailureResponse(message));
            }

            var response = new
            {
                User = user,
                Token = token
            };

            return Ok(ApiResponse<object>.SuccessResponse(response, message));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUser(long id)
        {
            var currentUserId = GetCurrentUserId();
            
            var success = await _authService.DeleteUserAsync(id, currentUserId);

            if (!success)
            {
                return BadRequest(ApiResponse.FailureResponse("Cannot delete user. Either user not found or you're trying to delete your own account."));
            }

            return Ok(ApiResponse.SuccessResponse("User deleted successfully"));
        }

        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
