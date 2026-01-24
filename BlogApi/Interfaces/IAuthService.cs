using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogApi.DTOs;
using BlogApi.Models;

namespace BlogApi.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, UserInfoDto? User, string? Token)> SignupAsync(SignupDto dto);
        Task<(bool Success, string Message, UserInfoDto? User, string? Token)> LoginAsync(LoginDto dto);

        Task<User?> GetUserByIdAsync(long id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<bool> DeleteUserAsync(long id, long currentUserId);
        string GenerateJwtToken(User user);

    }
}