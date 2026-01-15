using EcommerceApi.DTOs;
using EcommerceApi.Interfaces;
using EcommerceApi.Models;

namespace EcommerceApi.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(IUnitOfWork unitOfWork, ILogger<CategoryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();

            var categoryDtos = new List<CategoryResponseDto>();

            foreach (var category in categories)
            {
                var productCount = await _unitOfWork.Products.CountAsync(p => p.CategoryId == category.Id);

                categoryDtos.Add(new CategoryResponseDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    ProductCount = productCount
                });
            }

            return categoryDtos;
        }

        public async Task<CategoryDetailDto?> GetCategoryByIdAsync(long id)
        {
            var category = await _unitOfWork.Categories.GetCategoryWithProductsAsync(id);

            if (category == null)
                return null;

            return new CategoryDetailDto
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
        }

        public async Task<(bool Success, string Message, CategoryResponseDto? Category)> CreateCategoryAsync(CategoryDto dto)
        {
            try
            {
                if (await _unitOfWork.Categories.CategoryNameExistsAsync(dto.Name))
                {
                    return (false, "Category with this name already exists", null);
                }

                var category = new Category
                {
                    Name = dto.Name
                };

                await _unitOfWork.Categories.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

                var responseDto = new CategoryResponseDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    ProductCount = 0
                };

                _logger.LogInformation("Category {CategoryName} created successfully", dto.Name);

                return (true, "Category created successfully", responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category {CategoryName}", dto.Name);
                throw;
            }
        }

        public async Task<(bool Success, string Message)> UpdateCategoryAsync(long id, CategoryDto dto)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                {
                    return (false, "Category not found");
                }

                if (await _unitOfWork.Categories.CategoryNameExistsAsync(dto.Name, id))
                {
                    return (false, "Another category with this name already exists");
                }

                category.Name = dto.Name;

                await _unitOfWork.Categories.UpdateAsync(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Category {CategoryId} updated successfully", id);

                return (true, "Category updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", id);
                throw;
            }
        }

        public async Task<(bool Success, string Message)> DeleteCategoryAsync(long id)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                {
                    return (false, "Category not found");
                }

                if (await _unitOfWork.Categories.HasProductsAsync(id))
                {
                    return (false, "Cannot delete category with products. Remove products first.");
                }

                await _unitOfWork.Categories.DeleteAsync(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Category {CategoryId} deleted successfully", id);

                return (true, "Category deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", id);
                throw;
            }
        }
    }
}
