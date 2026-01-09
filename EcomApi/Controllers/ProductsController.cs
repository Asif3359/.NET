using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcomApi.Data;
using EcomApi.Models;
using EcomApi.DTOs;

namespace EcomApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication for all endpoints
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        [AllowAnonymous] // Allow anyone to view products
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Price > 0) // Example filter
                .OrderBy(p => p.Name)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : null
                })
                .ToListAsync();

            return Ok(products);
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        [AllowAnonymous] // Allow anyone to view specific product
        public async Task<ActionResult<ProductResponseDto>> GetProductById(long id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            var productDto = new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name
            };

            return Ok(productDto);
        }

        // POST: api/products
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only admin can create products
        public async Task<ActionResult<ProductResponseDto>> PostProduct([FromBody] ProductDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if product with same name exists
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.Name.ToLower() == productDto.Name.ToLower());

            if (existingProduct != null)
            {
                return BadRequest(new { message = "Product with this name already exists" });
            }

            // Validate category exists
            var category = await _context.Categories.FindAsync(productDto.CategoryId);
            if (category == null)
            {
                return BadRequest(new { message = "Category does not exist" });
            }

            var product = new Product
            {
                Name = productDto.Name,
                Price = productDto.Price,
                Description = productDto.Description,
                CategoryId = productDto.CategoryId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Get created product with category
            var createdProduct = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (createdProduct == null)
            {
                return NotFound(new { message = "Product not Created" });
            }

            var responseDto = new ProductResponseDto
            {
                Id = createdProduct.Id,
                Name = createdProduct.Name,
                Price = createdProduct.Price,
                Description = createdProduct.Description,
                CategoryId = createdProduct.CategoryId,
                CategoryName = createdProduct.Category?.Name
            };

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, responseDto);
        }

        // PUT: api/products/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Only admin can update products
        public async Task<IActionResult> PutProduct(long id, [FromBody] ProductDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            // Check if another product with same name exists
            var duplicateProduct = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id != id &&
                    p.Name.ToLower() == productDto.Name.ToLower());

            if (duplicateProduct != null)
            {
                return BadRequest(new { message = "Another product with this name already exists" });
            }

            // Validate category exists
            var category = await _context.Categories.FindAsync(productDto.CategoryId);
            if (category == null)
            {
                return BadRequest(new { message = "Category does not exist" });
            }

            product.Name = productDto.Name;
            product.Price = productDto.Price;
            product.Description = productDto.Description;
            product.CategoryId = productDto.CategoryId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound(new { message = "Product not found during update" });
                }
                throw;
            }

            return NoContent();
        }

        // PATCH: api/products/5 - Partial update
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")] // Only admin can partially update
        public async Task<IActionResult> PatchProduct(long id, [FromBody] UpdateProductDto updateDto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            // Update only provided fields
            if (updateDto.Name != null)
            {
                // Check for duplicate name
                var duplicateProduct = await _context.Products
                    .FirstOrDefaultAsync(p =>
                        p.Id != id &&
                        p.Name.ToLower() == updateDto.Name.ToLower());

                if (duplicateProduct != null)
                {
                    return BadRequest(new { message = "Another product with this name already exists" });
                }
                product.Name = updateDto.Name;
            }

            if (updateDto.Price.HasValue)
            {
                if (updateDto.Price.Value <= 0)
                    return BadRequest(new { message = "Price must be greater than 0" });
                product.Price = updateDto.Price.Value;
            }

            if (updateDto.Description != null)
            {
                product.Description = updateDto.Description;
            }

            if (updateDto.CategoryId.HasValue)
            {
                var category = await _context.Categories.FindAsync(updateDto.CategoryId.Value);
                if (category == null)
                {
                    return BadRequest(new { message = "Category does not exist" });
                }
                product.CategoryId = updateDto.CategoryId.Value;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                    return NotFound(new { message = "Product not found during update" });
                throw;
            }

            return NoContent();
        }

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only admin can delete products
        public async Task<IActionResult> DeleteProduct(long id)
        {
            var product = await _context.Products
                .Include(p => p.OrderItems)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            // Check if product has existing orders
            if (product.OrderItems.Any())
            {
                return BadRequest(new
                {
                    message = "Cannot delete product with existing orders. Consider disabling instead."
                });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/products/category/5 - Get products by category
        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProductsByCategory(long categoryId)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
            if (!categoryExists)
            {
                return NotFound(new { message = "Category not found" });
            }
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : null // Use ternary instead of ?.
                })
                .ToListAsync();

            return Ok(products);
        }

        private bool ProductExists(long id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}