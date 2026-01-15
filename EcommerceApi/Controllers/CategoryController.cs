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
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(ApiResponse<IEnumerable<CategoryResponseDto>>.SuccessResponse(categories, "Categories retrieved successfully"));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetCategory(long id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
            {
                return NotFound(ApiResponse.FailureResponse("Category not found"));
            }

            return Ok(ApiResponse<CategoryDetailDto>.SuccessResponse(category, "Category retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PostCategory([FromBody] CategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.FailureResponse("Validation failed", errors));
            }

            var (success, message, category) = await _categoryService.CreateCategoryAsync(dto);

            if (!success)
            {
                return BadRequest(ApiResponse.FailureResponse(message));
            }

            return CreatedAtAction(
                nameof(GetCategory), 
                new { id = category!.Id }, 
                ApiResponse<CategoryResponseDto>.SuccessResponse(category, message)
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PutCategory(long id, [FromBody] CategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.FailureResponse("Validation failed", errors));
            }

            var (success, message) = await _categoryService.UpdateCategoryAsync(id, dto);

            if (!success)
            {
                return NotFound(ApiResponse.FailureResponse(message));
            }

            return Ok(ApiResponse.SuccessResponse(message));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCategory(long id)
        {
            var (success, message) = await _categoryService.DeleteCategoryAsync(id);

            if (!success)
            {
                return BadRequest(ApiResponse.FailureResponse(message));
            }

            return Ok(ApiResponse.SuccessResponse(message));
        }
    }
}
