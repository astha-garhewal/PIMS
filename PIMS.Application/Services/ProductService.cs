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

    public async Task<List<ProductResponseDto>> SearchAsync(
        string? search,
        int? categoryId)
    {
        var products = await _productRepository.SearchAsync(
            search,
            categoryId);

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

    public async Task<ProductResponseDto?> AdjustPriceAsync(
        int productId,
        PriceAdjustmentDto dto)
    {
        var product = await _productRepository.GetByIdAsync(productId);

        if (product == null)
        {
            return null;
        }

        if (dto.Value < 0)
        {
            throw new ArgumentException(
                "Adjustment value cannot be negative.");
        }

        var adjustmentType = dto.AdjustmentType.Trim().ToLowerInvariant();

        if (adjustmentType == "fixed")
        {
            product.Price -= dto.Value;
        }
        else if (adjustmentType == "percentage")
        {
            product.Price -= product.Price * dto.Value / 100;
        }
        else
        {
            throw new ArgumentException(
                "Adjustment type must be 'fixed' or 'percentage'.");
        }

        if (product.Price < 0)
        {
            product.Price = 0;
        }

        product.UpdatedDate = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);

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

    public async Task<List<ProductResponseDto>> BulkAdjustPriceAsync(
        BulkPriceAdjustmentDto dto)
    {
        if (dto.ProductIds == null || !dto.ProductIds.Any())
        {
            throw new ArgumentException(
                "At least one product ID is required.");
        }

        if (dto.Value < 0)
        {
            throw new ArgumentException(
                "Adjustment value cannot be negative.");
        }

        var adjustmentType = dto.AdjustmentType
            .Trim()
            .ToLowerInvariant();

        if (adjustmentType != "fixed" &&
            adjustmentType != "percentage")
        {
            throw new ArgumentException(
                "Adjustment type must be 'fixed' or 'percentage'.");
        }

        var productIds = dto.ProductIds
            .Distinct()
            .ToList();

        var products = await _productRepository
            .GetByIdsAsync(productIds);

        if (products.Count != productIds.Count)
        {
            throw new ArgumentException(
                "One or more product IDs do not exist.");
        }

        foreach (var product in products)
        {
            if (adjustmentType == "fixed")
            {
                product.Price -= dto.Value;
            }
            else
            {
                product.Price -= product.Price * dto.Value / 100;
            }

            if (product.Price < 0)
            {
                product.Price = 0;
            }

            product.UpdatedDate = DateTime.UtcNow;
        }

        foreach (var product in products)
        {
            await _productRepository.UpdateAsync(product);
        }

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
}
