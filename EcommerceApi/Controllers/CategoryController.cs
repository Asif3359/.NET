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
using EcommerceApi.Models;
using EcommerceApi.Data;
using EcommerceApi.Enums;
using EcommerceApi.DTOs;
using System.Data.Common;

namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetCategorys()
        {
            var categories = await _context.Categories
                .Select(c => new CategoryResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ProductCount = c.Products.Count

                }).ToListAsync();

            return Ok(categories);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CategoryDetailDto>> GetCategory(long id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                return NotFound(
                    new
                    {
                        message = "Category not Found"
                    }
                );
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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoryResponseDto>> PostCategory([FromBody] CategoryDto dto)
        {
            if (await _context.Categories.AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower()))
            {
                return BadRequest(new
                {
                    message = "With this name category Already exists"
                });
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

            return CreatedAtAction(nameof(GetCategory), new
            {
                id = category.Id
            }, responseDto);


        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutCategory(long id, [FromBody] CategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }

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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCategory(long id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }

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