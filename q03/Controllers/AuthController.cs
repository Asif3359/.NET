using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using q03.Models;

namespace q03.Controllers
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
                id = user.Id,
                email = user.Email
            });

        }

        [HttpPost("Signup")]
        public async Task<ActionResult> Signup(UserSignupDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("User already exists");

            var user = new User
            {
                Email = dto.Email,
                Password = dto.Password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
            {
                id = user.Id,
                email = user.Email,
                message = "Signup successful"
            });
        }

        [HttpPost("Login")]
        public async Task<ActionResult> Login(UserSignupDto dto)
        {
            var existingUser = _context.Users
                .FirstOrDefault(u => u.Email == dto.Email && u.Password == dto.Password);

            if (existingUser == null)
                return Unauthorized("Invalid credentials");

            return Ok(new
            {
                id = existingUser.Id,
                email = existingUser.Email,
                message = "Login successful"
            });
        }

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
