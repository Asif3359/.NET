using EcommerceApi.DTOs;
using EcommerceApi.Interfaces;
using EcommerceApi.Models;

namespace EcommerceApi.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync()
        {
            var products = await _unitOfWork.Products.GetProductsWithCategoryAsync();

            return products
                .Where(p => p.Price > 0)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name
                });
        }

        public async Task<ProductResponseDto?> GetProductByIdAsync(long id)
        {
            var product = await _unitOfWork.Products.GetProductWithCategoryAsync(id);

            if (product == null)
                return null;

            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name
            };
        }

        public async Task<IEnumerable<ProductResponseDto>> GetProductsByCategoryAsync(long categoryId)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category == null)
            {
                return Enumerable.Empty<ProductResponseDto>();
            }

            var products = await _unitOfWork.Products.GetProductsByCategoryAsync(categoryId);

            return products.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name
            });
        }

        public async Task<(bool Success, string Message, ProductResponseDto? Product)> CreateProductAsync(ProductDto dto)
        {
            try
            {
                // Check if product with same name exists
                if (await _unitOfWork.Products.ProductNameExistsAsync(dto.Name))
                {
                    return (false, "Product with same name already exists", null);
                }

                // Check if category exists
                var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
                if (category == null)
                {
                    return (false, "Category does not exist", null);
                }

                var product = new Product
                {
                    Name = dto.Name,
                    Price = dto.Price,
                    Description = dto.Description,
                    CategoryId = dto.CategoryId
                };

                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();

                // Reload with category
                var createdProduct = await _unitOfWork.Products.GetProductWithCategoryAsync(product.Id);

                var productDto = new ProductResponseDto
                {
                    Id = createdProduct!.Id,
                    Name = createdProduct.Name,
                    Price = createdProduct.Price,
                    Description = createdProduct.Description,
                    CategoryId = createdProduct.CategoryId,
                    CategoryName = createdProduct.Category?.Name
                };

                _logger.LogInformation("Product {ProductName} created successfully", dto.Name);

                return (true, "Product created successfully", productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product {ProductName}", dto.Name);
                throw;
            }
        }

        public async Task<(bool Success, string Message)> UpdateProductAsync(long id, ProductDto dto)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                {
                    return (false, "Product not found");
                }

                // Check for duplicate name
                if (await _unitOfWork.Products.ProductNameExistsAsync(dto.Name, id))
                {
                    return (false, "Another product with same name exists");
                }

                // Check if category exists
                var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
                if (category == null)
                {
                    return (false, "Category does not exist");
                }

                product.Name = dto.Name;
                product.Price = dto.Price;
                product.Description = dto.Description;
                product.CategoryId = dto.CategoryId;

                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Product {ProductId} updated successfully", id);

                return (true, "Product updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                throw;
            }
        }

        public async Task<(bool Success, string Message)> DeleteProductAsync(long id)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                {
                    return (false, "Product not found");
                }

                // Check if product is in any orders (this would need OrderItems check)
                // For now, we'll just delete it
                await _unitOfWork.Products.DeleteAsync(product);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Product {ProductId} deleted successfully", id);

                return (true, "Product deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                throw;
            }
        }
    }
}
