using Moq;
using PIMS.Application.DTOs.Inventory;
using PIMS.Application.Interfaces;
using PIMS.Application.Services;
using PIMS.Domain.Entities;

namespace PIMS.Tests;

public class InventoryServiceTests
{
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly InventoryService _inventoryService;

    public InventoryServiceTests()
    {
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        var productRepositoryMock = new Mock<IProductRepository>();
        var alertRepositoryMock = new Mock<ILowInventoryAlertRepository>();
        var alertService = new LowInventoryAlertService(
            alertRepositoryMock.Object);

        _inventoryService = new InventoryService(
            _inventoryRepositoryMock.Object,
            productRepositoryMock.Object,
            alertService);
    }

    [Fact]
    public async Task ProcessTransactionAsync_Add_IncreasesQuantity()
    {
        var inventory = CreateInventory(100, 10);
        _inventoryRepositoryMock
            .Setup(repository => repository.GetByIdAsync(1))
            .ReturnsAsync(inventory);

        var result = await _inventoryService.ProcessTransactionAsync(
            1,
            new InventoryTransactionDto
            {
                Quantity = 50,
                TransactionType = "ADD",
                Reason = "New stock"
            },
            1);

        Assert.Equal(150, result.CurrentQuantity);
        Assert.Equal(50, result.QuantityChange);
        Assert.Equal("ADD", result.TransactionType);
    }

    [Fact]
    public async Task ProcessTransactionAsync_Subtract_DecreasesQuantity()
    {
        var inventory = CreateInventory(100, 10);
        _inventoryRepositoryMock
            .Setup(repository => repository.GetByIdAsync(1))
            .ReturnsAsync(inventory);

        var result = await _inventoryService.ProcessTransactionAsync(
            1,
            new InventoryTransactionDto
            {
                Quantity = 30,
                TransactionType = "SUBTRACT",
                Reason = "Sale"
            },
            1);

        Assert.Equal(70, result.CurrentQuantity);
        Assert.Equal(-30, result.QuantityChange);
    }

    [Fact]
    public async Task ProcessTransactionAsync_SubtractMoreThanAvailable_ThrowsException()
    {
        var inventory = CreateInventory(20, 5);
        _inventoryRepositoryMock
            .Setup(repository => repository.GetByIdAsync(1))
            .ReturnsAsync(inventory);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _inventoryService.ProcessTransactionAsync(
                1,
                new InventoryTransactionDto
                {
                    Quantity = 50,
                    TransactionType = "SUBTRACT"
                },
                1));

        Assert.Equal(20, inventory.Quantity);
    }

    [Fact]
    public async Task ProcessTransactionAsync_InvalidTransactionType_ThrowsException()
    {
        var inventory = CreateInventory(100, 10);
        _inventoryRepositoryMock
            .Setup(repository => repository.GetByIdAsync(1))
            .ReturnsAsync(inventory);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _inventoryService.ProcessTransactionAsync(
                1,
                new InventoryTransactionDto
                {
                    Quantity = 10,
                    TransactionType = "INVALID"
                },
                1));
    }

    [Fact]
    public async Task PerformAuditAsync_CalculatesDifferenceCorrectly()
    {
        var inventory = CreateInventory(55, 10);
        _inventoryRepositoryMock
            .Setup(repository => repository.GetByIdAsync(1))
            .ReturnsAsync(inventory);

        var result = await _inventoryService.PerformAuditAsync(
            1,
            new InventoryAuditDto
            {
                AdjustedQuantity = 40,
                Reason = "Physical stock count"
            },
            1);

        Assert.Equal(55, result.PreviousQuantity);
        Assert.Equal(40, result.AdjustedQuantity);
        Assert.Equal(-15, result.Difference);
    }

    private static Inventory CreateInventory(
        int quantity,
        int lowStockThreshold)
    {
        return new Inventory
        {
            InventoryID = 1,
            ProductID = 1,
            Quantity = quantity,
            LowStockThreshold = lowStockThreshold,
            Product = new Product
            {
                ProductID = 1,
                SKU = "LAP-001",
                Name = "Dell Laptop"
            }
        };
    }
}
