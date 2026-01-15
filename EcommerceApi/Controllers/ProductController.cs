using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.DTOs;
using EcommerceApi.Helpers;
using EcommerceApi.Interfaces;

namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(ApiResponse<IEnumerable<ProductResponseDto>>.SuccessResponse(products, "Products retrieved successfully"));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetProductById(long id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound(ApiResponse.FailureResponse("Product not found"));
            }

            return Ok(ApiResponse<ProductResponseDto>.SuccessResponse(product, "Product retrieved successfully"));
        }

        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetProductsByCategory(long categoryId)
        {
            var products = await _productService.GetProductsByCategoryAsync(categoryId);

            if (!products.Any())
            {
                return NotFound(ApiResponse.FailureResponse("No products found for this category or category does not exist"));
            }

            return Ok(ApiResponse<IEnumerable<ProductResponseDto>>.SuccessResponse(products, "Products retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PostProduct([FromBody] ProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.FailureResponse("Validation failed", errors));
            }

            var (success, message, product) = await _productService.CreateProductAsync(dto);

            if (!success)
            {
                return BadRequest(ApiResponse.FailureResponse(message));
            }

            return CreatedAtAction(
                nameof(GetProductById), 
                new { id = product!.Id }, 
                ApiResponse<ProductResponseDto>.SuccessResponse(product, message)
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PutProduct(long id, [FromBody] ProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.FailureResponse("Validation failed", errors));
            }

            var (success, message) = await _productService.UpdateProductAsync(id, dto);

            if (!success)
            {
                return NotFound(ApiResponse.FailureResponse(message));
            }

            return Ok(ApiResponse.SuccessResponse(message));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteProduct(long id)
        {
            var (success, message) = await _productService.DeleteProductAsync(id);

            if (!success)
            {
                return NotFound(ApiResponse.FailureResponse(message));
            }

            return Ok(ApiResponse.SuccessResponse(message));
        }
    }
}
