using EcommerceApi.DTOs;

namespace EcommerceApi.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync();
        Task<CategoryDetailDto?> GetCategoryByIdAsync(long id);
        Task<(bool Success, string Message, CategoryResponseDto? Category)> CreateCategoryAsync(CategoryDto dto);
        Task<(bool Success, string Message)> UpdateCategoryAsync(long id, CategoryDto dto);
        Task<(bool Success, string Message)> DeleteCategoryAsync(long id);
    }
}
