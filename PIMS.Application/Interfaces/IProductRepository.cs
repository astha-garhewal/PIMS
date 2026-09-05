using PIMS.Domain.Entities;

namespace PIMS.Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int productId);

    Task<Product?> GetBySkuAsync(string sku);

    Task<List<Product>> GetAllAsync();

    Task AddAsync(Product product);

    Task UpdateAsync(Product product);

    Task DeleteAsync(Product product);

    Task AddProductCategoriesAsync(List<ProductCategory> productCategories);
}
