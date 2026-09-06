using Microsoft.Extensions.Caching.Memory;
using Moq;
using PIMS.Application.DTOs.Products;
using PIMS.Application.Interfaces;
using PIMS.Application.Services;
using PIMS.Domain.Entities;

namespace PIMS.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        var cacheMock = new Mock<IMemoryCache>();

        _productService = new ProductService(
            _productRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            cacheMock.Object);
    }

    [Fact]
    public async Task AdjustPriceAsync_FixedAdjustment_ReducesPrice()
    {
        var product = CreateProduct(1000);
        _productRepositoryMock
            .Setup(repository => repository.GetByIdAsync(1))
            .ReturnsAsync(product);

        var result = await _productService.AdjustPriceAsync(
            1,
            new PriceAdjustmentDto
            {
                Value = 100,
                AdjustmentType = "fixed"
            });

        Assert.NotNull(result);
        Assert.Equal(900, result!.Price);
    }

    [Fact]
    public async Task AdjustPriceAsync_PercentageAdjustment_ReducesPrice()
    {
        var product = CreateProduct(1000);
        _productRepositoryMock
            .Setup(repository => repository.GetByIdAsync(1))
            .ReturnsAsync(product);

        var result = await _productService.AdjustPriceAsync(
            1,
            new PriceAdjustmentDto
            {
                Value = 10,
                AdjustmentType = "percentage"
            });

        Assert.NotNull(result);
        Assert.Equal(900, result!.Price);
    }

    [Fact]
    public async Task AdjustPriceAsync_PriceCannotBecomeNegative()
    {
        var product = CreateProduct(500);
        _productRepositoryMock
            .Setup(repository => repository.GetByIdAsync(1))
            .ReturnsAsync(product);

        var result = await _productService.AdjustPriceAsync(
            1,
            new PriceAdjustmentDto
            {
                Value = 1000,
                AdjustmentType = "fixed"
            });

        Assert.NotNull(result);
        Assert.Equal(0, result!.Price);
    }

    [Fact]
    public async Task AdjustPriceAsync_ProductDoesNotExist_ReturnsNull()
    {
        _productRepositoryMock
            .Setup(repository => repository.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        var result = await _productService.AdjustPriceAsync(
            999,
            new PriceAdjustmentDto
            {
                Value = 100,
                AdjustmentType = "fixed"
            });

        Assert.Null(result);
    }

    [Fact]
    public async Task AdjustPriceAsync_InvalidAdjustmentType_ThrowsException()
    {
        var product = CreateProduct(1000);
        _productRepositoryMock
            .Setup(repository => repository.GetByIdAsync(1))
            .ReturnsAsync(product);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _productService.AdjustPriceAsync(
                1,
                new PriceAdjustmentDto
                {
                    Value = 100,
                    AdjustmentType = "random"
                }));
    }

    [Fact]
    public async Task CreateAsync_Should_Reject_Invalid_Category()
    {
        var dto = new CreateProductDto
        {
            SKU = "TEST-INVALID-CATEGORY",
            Name = "Test Product",
            Price = 100,
            CategoryIds = new List<int> { 99999 }
        };

        _productRepositoryMock
            .Setup(repository => repository.GetBySkuAsync(dto.SKU))
            .ReturnsAsync((Product?)null);

        _categoryRepositoryMock
            .Setup(repository => repository.GetByIdAsync(99999))
            .ReturnsAsync((Category?)null);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _productService.CreateAsync(dto));
    }

    [Fact]
    public async Task BulkAdjustPriceAsync_UsesSingleRangeUpdate()
    {
        var products = new List<Product>
        {
            CreateProduct(1000),
            new Product
            {
                ProductID = 2,
                SKU = "LAP-002",
                Name = "HP Laptop",
                Price = 2000
            }
        };

        _productRepositoryMock
            .Setup(repository => repository.GetByIdsAsync(
                It.IsAny<List<int>>()))
            .ReturnsAsync(products);

        _productRepositoryMock
            .Setup(repository => repository.UpdateRangeAsync(
                It.IsAny<List<Product>>()))
            .Returns(Task.CompletedTask);

        var result = await _productService.BulkAdjustPriceAsync(
            new BulkPriceAdjustmentDto
            {
                ProductIds = new List<int> { 1, 2 },
                Value = 10,
                AdjustmentType = "percentage"
            });

        Assert.Equal(900, result[0].Price);
        Assert.Equal(1800, result[1].Price);
        _productRepositoryMock.Verify(
            repository => repository.UpdateRangeAsync(
                It.Is<List<Product>>(updatedProducts =>
                    updatedProducts.Count == 2)),
            Times.Once);
        _productRepositoryMock.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<Product>()),
            Times.Never);
    }

    private static Product CreateProduct(decimal price)
    {
        return new Product
        {
            ProductID = 1,
            SKU = "LAP-001",
            Name = "Dell Laptop",
            Price = price
        };
    }
}
