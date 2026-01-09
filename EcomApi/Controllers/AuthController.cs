using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcomApi.Models;
using EcomApi.Data;
using EcomApi.DTOs;
using EcomApi.Enums;
using Microsoft.VisualBasic;
using BCrypt.Net;
using System.Reflection.Metadata;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EcomApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {

            return await _context.Users.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role,
                Orders = user.Orders
            });
        }

        [HttpPost("Signup")]
        public async Task<ActionResult> Signup([FromBody] SignupDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("User exists");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = HashPassword(dto.Password),
                Role = UserRole.User
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Id = user.Id,
                Email = user.Email,
                Message = "Signup successful"
            });
        }


        [HttpPost("Login")]
        public async Task<ActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                return Unauthorized("Invalid email or password");
            }

            if (!VerifyPassword(dto.Password, user.Password))
            {
                return Unauthorized("Invalid email or password");
            }

            return Ok(new
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role,
                Message = "Login successful"
            });
        }

        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutTModel(int Id, TModel model)
        // {
        //     // TODO: Your code here
        //     await Task.Yield();

        //     return NoContent();
        // }

        // [HttpDelete("{id}")]
        // public async Task<ActionResult<TModel>> DeleteTModelById(int Id)
        // {
        //     // TODO: Your code here
        //     await Task.Yield();

        //     return null;
        // }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}