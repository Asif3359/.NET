using EcommerceApi.DTOs;

namespace EcommerceApi.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync();
        Task<ProductResponseDto?> GetProductByIdAsync(long id);
        Task<IEnumerable<ProductResponseDto>> GetProductsByCategoryAsync(long categoryId);
        Task<(bool Success, string Message, ProductResponseDto? Product)> CreateProductAsync(ProductDto dto);
        Task<(bool Success, string Message)> UpdateProductAsync(long id, ProductDto dto);
        Task<(bool Success, string Message)> DeleteProductAsync(long id);
    }
}
