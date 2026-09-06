using Microsoft.Extensions.Caching.Memory;
using PIMS.Application.DTOs.Products;
using PIMS.Application.Interfaces;
using PIMS.Domain.Entities;

namespace PIMS.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMemoryCache _cache;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IMemoryCache cache)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _cache = cache;
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

        foreach (var categoryId in dto.CategoryIds.Distinct())
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);

            if (category == null)
            {
                throw new ArgumentException(
                    $"Category with ID {categoryId} does not exist.");
            }
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
        var cacheKey = $"product_{productId}";

        if (_cache.TryGetValue(
            cacheKey,
            out ProductResponseDto? cachedProduct))
        {
            return cachedProduct;
        }

        var product = await _productRepository.GetByIdAsync(productId);

        if (product == null)
        {
            return null;
        }

        var response = new ProductResponseDto
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

            _cache.Set(
                cacheKey,
                response,
                TimeSpan.FromMinutes(5));

            return response;
    }

    public async Task<ProductResponseDto?> UpdateAsync(
        int productId,
        UpdateProductDto dto)
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

        var product = await _productRepository.GetByIdAsync(productId);

        if (product == null)
        {
            return null;
        }

        var existingProduct = await _productRepository.GetBySkuAsync(dto.SKU);

        if (existingProduct != null && existingProduct.ProductID != productId)
        {
            throw new ArgumentException("SKU already exists.");
        }

        product.SKU = dto.SKU.Trim();
        product.Name = dto.Name.Trim();
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.UpdatedDate = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);
        _cache.Remove($"product_{product.ProductID}");

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
        _cache.Remove($"product_{product.ProductID}");

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
            decimal deduction;

            if (adjustmentType == "percentage")
            {
                deduction = product.Price * dto.Value / 100;
            }
            else
            {
                deduction = dto.Value;
            }

            product.Price = Math.Max(0, product.Price - deduction);
            product.UpdatedDate = DateTime.UtcNow;
        }

        await _productRepository.UpdateRangeAsync(products);

        foreach (var product in products)
        {
            _cache.Remove($"product_{product.ProductID}");
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
