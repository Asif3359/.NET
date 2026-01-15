using EcommerceApi.Models;

namespace EcommerceApi.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetCategoryWithProductsAsync(long id);
        Task<bool> CategoryNameExistsAsync(string name, long? excludeId = null);
        Task<bool> HasProductsAsync(long categoryId);
    }
}
