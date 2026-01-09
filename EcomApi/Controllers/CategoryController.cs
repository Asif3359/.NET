using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Reflection.Metadata;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcomApi.Models;
using EcomApi.Data;
using EcomApi.DTOs;
using EcomApi.Enums;

namespace EcomApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication for all endpoints
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/category
        [HttpGet]
        [AllowAnonymous] // Allow anyone to view categories
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c => new CategoryResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ProductCount = c.Products.Count
                })
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/category/5
        [HttpGet("{id}")]
        [AllowAnonymous] // Allow anyone to view specific category
        public async Task<ActionResult<CategoryDetailDto>> GetCategory(long id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            var categoryDto = new CategoryDetailDto
            {
                Id = category.Id,
                Name = category.Name,
                Products = category.Products.Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Description = p.Description
                }).ToList()
            };

            return Ok(categoryDto);
        }

        // POST: api/category
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only admin can create categories
        public async Task<ActionResult<CategoryResponseDto>> PostCategory([FromBody] CategoryDto dto)
        {
            // Check if category with same name exists
            if (await _context.Categories.AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower()))
            {
                return BadRequest(new { message = "Category with this name already exists" });
            }

            var category = new Category
            {
                Name = dto.Name
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var responseDto = new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                ProductCount = 0
            };

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, responseDto);
        }

        // PUT: api/category/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Only admin can update categories
        public async Task<IActionResult> PutCategory(long id, [FromBody] CategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            // Check if another category with same name exists
            if (await _context.Categories.AnyAsync(c =>
                c.Id != id && c.Name.ToLower() == dto.Name.ToLower()))
            {
                return BadRequest(new { message = "Another category with this name already exists" });
            }

            category.Name = dto.Name;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                    return NotFound(new { message = "Category not found" });
                throw;
            }

            return NoContent();
        }

        // DELETE: api/category/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only admin can delete categories
        public async Task<IActionResult> DeleteCategory(long id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            // Check if category has products
            if (category.Products.Any())
            {
                return BadRequest(new
                {
                    message = "Cannot delete category with products. Remove products first."
                });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(long id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}