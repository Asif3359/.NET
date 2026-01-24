using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BlogApi.DTOs;
using BlogApi.Enums;
using BlogApi.Helpers;
using BlogApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers
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

        [HttpPost("signup")]
        public async Task<ActionResult> Signup([FromBody] SignupDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<object>.FailureResponse("Validation failed", errors));
            }

            try
            {
                var (success, message, user, token) = await _authService.SignupAsync(dto);

                if (!success)
                {
                    return BadRequest(ApiResponse<object>.FailureResponse(message));
                }

                var response = new
                {
                    User = user,
                    Token = token
                };

                return Ok(ApiResponse<object>.SuccessResponse(response, message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during signup");
                return StatusCode(500, ApiResponse<object>.FailureResponse("An error occurred while processing your request"));
            }
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
                return BadRequest(ApiResponse<object>.FailureResponse("Validation failed", errors));
            }

            try
            {
                var (success, message, user, token) = await _authService.LoginAsync(dto);

                if (!success)
                {
                    return Unauthorized(ApiResponse<object>.FailureResponse(message));
                }

                var response = new
                {
                    User = user,
                    Token = token
                };

                return Ok(ApiResponse<object>.SuccessResponse(response, message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, ApiResponse<object>.FailureResponse("An error occurred while processing your request"));
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult> GetCurrentUser()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(ApiResponse<UserInfoDto>.FailureResponse("Invalid token"));
                }

                var user = await _authService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(ApiResponse<UserInfoDto>.FailureResponse("User not found"));
                }

                var userInfo = new UserInfoDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                };

                return Ok(ApiResponse<UserInfoDto>.SuccessResponse(userInfo, "User retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user");
                return StatusCode(500, ApiResponse<UserInfoDto>.FailureResponse("An error occurred while processing your request"));
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetAllUsers()
        {
            try
            {
                var users = await _authService.GetAllUsersAsync();

                var userDtos = users.Select(u => new UserInfoDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role
                });

                return Ok(ApiResponse<IEnumerable<UserInfoDto>>.SuccessResponse(userDtos, "Users retrive successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Retrieving users");
                return StatusCode(500, ApiResponse<IEnumerable<UserInfoDto>>.FailureResponse("An error occurred while processing your request"));
            }
        }


        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult> GetUser(long id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return Unauthorized(ApiResponse<UserInfoDto>.FailureResponse("Invalid token"));
                }

                var currentUser = await _authService.GetUserByIdAsync(currentUserId);

                // Check authorization: Admin can view any user, others can only view themselves
                if (currentUser?.Role != UserRole.Admin && currentUserId != id)
                {
                    return Forbid();
                }

                var user = await _authService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserInfoDto>.FailureResponse("User not found"));
                }

                var userInfo = new UserInfoDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                };

                return Ok(ApiResponse<UserInfoDto>.SuccessResponse(userInfo, "User retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", id);
                return StatusCode(500, ApiResponse<UserInfoDto>.FailureResponse("An error occurred while processing your request"));
            }
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUser(long id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return Unauthorized(ApiResponse.FailureResponse("Invalid token"));
                }

                var success = await _authService.DeleteUserAsync(id, currentUserId);

                if (!success)
                {
                    return BadRequest(ApiResponse.FailureResponse("Cannot delete user. Either user not found or you're trying to delete your own account."));
                }

                return Ok(ApiResponse.SuccessResponse("User deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, ApiResponse.FailureResponse("An error occurred while processing your request"));
            }
        }


        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value; // JWT standard claim

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return 0;
            }

            return long.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}