using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using EcommerceApi.DTOs;
using EcommerceApi.Enums;
using EcommerceApi.Helpers;
using EcommerceApi.Interfaces;
using EcommerceApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EcommerceApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUnitOfWork unitOfWork,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, UserInfoDto? User, string? Token)> SignupAsync(SignupDto dto)
        {
            try
            {
                // Check if user already exists
                if (await _unitOfWork.Users.EmailExistsAsync(dto.Email))
                {
                    return (false, "User with this email already exists", null, null);
                }

                var user = new User
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Role = UserRole.User
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var token = GenerateJwtToken(user);

                var userInfo = new UserInfoDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString()
                };

                _logger.LogInformation("User {Email} registered successfully", dto.Email);

                return (true, "Signup successful", userInfo, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during signup for {Email}", dto.Email);
                throw;
            }
        }

        public async Task<(bool Success, string Message, UserInfoDto? User, string? Token)> LoginAsync(LoginDto dto)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                {
                    _logger.LogWarning("Failed login attempt for {Email}", dto.Email);
                    return (false, "Invalid email or password", null, null);
                }

                var token = GenerateJwtToken(user);

                var userInfo = new UserInfoDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString()
                };

                _logger.LogInformation("User {Email} logged in successfully", dto.Email);

                return (true, "Login successful", userInfo, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", dto.Email);
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(long id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _unitOfWork.Users.GetByEmailAsync(email);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _unitOfWork.Users.GetAllAsync();
        }

        public async Task<bool> DeleteUserAsync(long id, long currentUserId)
        {
            if (id == currentUserId)
            {
                return false; // Cannot delete own account
            }

            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            await _unitOfWork.Users.DeleteAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User {UserId} deleted by {CurrentUserId}", id, currentUserId);

            return true;
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
