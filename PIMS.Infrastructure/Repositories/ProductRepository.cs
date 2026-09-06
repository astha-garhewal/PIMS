using Microsoft.EntityFrameworkCore;
using PIMS.Application.Interfaces;
using PIMS.Domain.Entities;
using PIMS.Infrastructure.Data;

namespace PIMS.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int productId)
    {
        return await _context.Products
            .Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Category)
            .FirstOrDefaultAsync(p => p.ProductID == productId);
    }

    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.SKU == sku);
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Category)
            .OrderBy(p => p.ProductID)
            .ToListAsync();
    }

    public async Task<List<Product>> SearchAsync(
        string? search,
        int? categoryId)
    {
        var query = _context.Products
            .Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(p =>
                p.Name.Contains(search) ||
                p.SKU.Contains(search));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p =>
                p.ProductCategories
                    .Any(pc => pc.CategoryID == categoryId.Value));
        }

        return await query
            .OrderBy(p => p.ProductID)
            .ToListAsync();
    }

    public async Task<List<Product>> GetByIdsAsync(
        List<int> productIds)
    {
        return await _context.Products
            .Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Category)
            .Where(p => productIds.Contains(p.ProductID))
            .ToListAsync();
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(List<Product> products)
    {
        _context.Products.UpdateRange(products);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    public async Task AddProductCategoriesAsync(
        List<ProductCategory> productCategories)
    {
        await _context.ProductCategories.AddRangeAsync(productCategories);

        await _context.SaveChangesAsync();
    }
}
