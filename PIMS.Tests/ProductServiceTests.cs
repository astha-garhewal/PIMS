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
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        var cacheMock = new Mock<IMemoryCache>();

        _productService = new ProductService(
            _productRepositoryMock.Object,
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
