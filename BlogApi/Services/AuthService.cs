using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BlogApi.DTOs;
using BlogApi.Helpers;
using BlogApi.Interfaces;
using BlogApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BlogApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthService> _logger;
        private readonly JwtSettings _jwtSettings;


        public AuthService(
            IUnitOfWork unitOfWork,
            ILogger<AuthService> logger,
            IOptions<JwtSettings> jwtSettings
        )
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _jwtSettings = jwtSettings.Value;

        }

        public async Task<bool> DeleteUserAsync(long id, long currentUserId)
        {
            if (id == currentUserId)
            {
                return false;
            }

            var user = await _unitOfWork.Users.GetByIdAsync(id);

            if (user == null)
            {
                return false;
            }

            await _unitOfWork.Users.DeleteAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }


        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _unitOfWork.Users.GetAllAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _unitOfWork.Users.GetByEmailAsync(email);
        }

        public async Task<User?> GetUserByIdAsync(long id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        public async Task<(bool Success, string Message, UserInfoDto? User, string? Token)> LoginAsync(LoginDto dto)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
                if (user == null)
                {
                    return (false, "Invalid email or password", null, null);
                }

                if (!VerifyPassword(dto.Password, user.Password))
                {
                    return (false, "Invalid email or password", null, null);
                }

                var token = GenerateJwtToken(user);

                var userDto = new UserInfoDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                };

                _logger.LogInformation("User {Email} logged in successfully", dto.Email);

                return (true, "Login successful", userDto, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", dto.Email);
                throw;
            }
        }

        public async Task<(bool Success, string Message, UserInfoDto? User, string? Token)> SignupAsync(SignupDto dto)
        {
            try
            {
                if (await _unitOfWork.Users.EmailExistsAsync(dto.Email))
                {
                    return (false, "Email already exist", null, null);
                }

                var hashedPassword = HashPassword(dto.Password);

                var user = new User
                {
                    Name = dto.Name,
                    Email = dto.Email.ToLower(),
                    Password = hashedPassword,
                    Role = Enums.UserRole.User,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();


                var token = GenerateJwtToken(user);

                var userDto = new UserInfoDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                };


                _logger.LogInformation("User {Email} loged in successfully");


                return (true, "User created successfully", userDto, token);

            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error during login for {Email}", dto.Email);
                throw;
            }
        }

        public string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique token ID
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                signingCredentials: creds,
                notBefore: DateTime.UtcNow
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

    }
}