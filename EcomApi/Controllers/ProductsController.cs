using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcomApi.Data;
using EcomApi.Models;
using EcomApi.DTOs;

namespace EcomApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
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
        public async Task<ActionResult<ProductResponseDto>> PostProduct([FromBody] ProductDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == productDto.CategoryId);
            if (!categoryExists)
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

            var createdProduct = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

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

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == productDto.CategoryId);
            if (!categoryExists)
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
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(long id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(long id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}