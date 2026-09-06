using PIMS.Domain.Entities;

namespace PIMS.Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int productId);

    Task<Product?> GetBySkuAsync(string sku);

    Task<List<Product>> GetAllAsync();

    Task<List<Product>> SearchAsync(
        string? search,
        int? categoryId);

    Task<List<Product>> GetByIdsAsync(List<int> productIds);

    Task AddAsync(Product product);

    Task UpdateAsync(Product product);

    Task UpdateRangeAsync(List<Product> products);

    Task DeleteAsync(Product product);

    Task AddProductCategoriesAsync(List<ProductCategory> productCategories);
}
