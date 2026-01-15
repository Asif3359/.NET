using EcommerceApi.Models;

namespace EcommerceApi.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(long categoryId);
        Task<IEnumerable<Product>> GetProductsWithCategoryAsync();
        Task<Product?> GetProductWithCategoryAsync(long id);
        Task<bool> ProductNameExistsAsync(string name, long? excludeId = null);
    }
}
