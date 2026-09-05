using PIMS.Application.DTOs.Products;
using PIMS.Application.Interfaces;
using PIMS.Domain.Entities;

namespace PIMS.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.SKU))
        {
            throw new ArgumentException("SKU is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException("Product name is required.");
        }

        if (dto.Price < 0)
        {
            throw new ArgumentException("Product price cannot be negative.");
        }

        var existingProduct = await _productRepository.GetBySkuAsync(dto.SKU);

        if (existingProduct != null)
        {
            throw new ArgumentException("SKU already exists.");
        }

        var product = new Product
        {
            SKU = dto.SKU,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            CreatedDate = DateTime.UtcNow
        };

        await _productRepository.AddAsync(product);

        if (dto.CategoryIds.Any())
        {
            var productCategories = dto.CategoryIds
                .Distinct()
                .Select(categoryId => new ProductCategory
                {
                    ProductID = product.ProductID,
                    CategoryID = categoryId
                })
                .ToList();

            await _productRepository.AddProductCategoriesAsync(
                productCategories);
        }

        return new ProductResponseDto
        {
            ProductID = product.ProductID,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CreatedDate = product.CreatedDate
        };
    }

    public async Task<List<ProductResponseDto>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();

        return products.Select(p => new ProductResponseDto
        {
            ProductID = p.ProductID,
            SKU = p.SKU,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            CreatedDate = p.CreatedDate,
            Categories = p.ProductCategories
                .Select(pc => pc.Category.CategoryName)
                .ToList()
        }).ToList();
    }

    public async Task<ProductResponseDto?> GetByIdAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);

        if (product == null)
        {
            return null;
        }

        return new ProductResponseDto
        {
            ProductID = product.ProductID,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CreatedDate = product.CreatedDate,
            Categories = product.ProductCategories
                .Select(pc => pc.Category.CategoryName)
                .ToList()
        };
    }
}
