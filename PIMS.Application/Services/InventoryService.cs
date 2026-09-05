using PIMS.Application.DTOs.Inventory;
using PIMS.Application.Interfaces;
using PIMS.Domain.Entities;

namespace PIMS.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductRepository _productRepository;

    public InventoryService(
        IInventoryRepository inventoryRepository,
        IProductRepository productRepository)
    {
        _inventoryRepository = inventoryRepository;
        _productRepository = productRepository;
    }

    public async Task<InventoryResponseDto> CreateAsync(CreateInventoryDto dto)
    {
        if (dto.ProductID <= 0)
        {
            throw new ArgumentException("Product ID is required.");
        }

        if (dto.Quantity < 0)
        {
            throw new ArgumentException("Quantity cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(dto.WarehouseLocation))
        {
            throw new ArgumentException("Warehouse location is required.");
        }

        if (dto.LowStockThreshold < 0)
        {
            throw new ArgumentException("Low stock threshold cannot be negative.");
        }

        var product = await _productRepository.GetByIdAsync(dto.ProductID);

        if (product == null)
        {
            throw new ArgumentException("Product does not exist.");
        }

        var existingInventory =
            await _inventoryRepository.GetByProductIdAsync(dto.ProductID);

        if (existingInventory != null)
        {
            throw new ArgumentException(
                "Inventory already exists for this product.");
        }

        var inventory = new Inventory
        {
            ProductID = dto.ProductID,
            Quantity = dto.Quantity,
            WarehouseLocation = dto.WarehouseLocation.Trim(),
            LowStockThreshold = dto.LowStockThreshold
        };

        await _inventoryRepository.AddAsync(inventory);

        return new InventoryResponseDto
        {
            InventoryID = inventory.InventoryID,
            ProductID = product.ProductID,
            SKU = product.SKU,
            ProductName = product.Name,
            Quantity = inventory.Quantity,
            WarehouseLocation = inventory.WarehouseLocation,
            LowStockThreshold = inventory.LowStockThreshold
        };
    }

    public async Task<InventoryResponseDto?> GetByIdAsync(int inventoryId)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(inventoryId);

        if (inventory == null)
        {
            return null;
        }

        return new InventoryResponseDto
        {
            InventoryID = inventory.InventoryID,
            ProductID = inventory.ProductID,
            SKU = inventory.Product.SKU,
            ProductName = inventory.Product.Name,
            Quantity = inventory.Quantity,
            WarehouseLocation = inventory.WarehouseLocation,
            LowStockThreshold = inventory.LowStockThreshold
        };
    }

    public async Task<InventoryTransactionResponseDto> ProcessTransactionAsync(
        int inventoryId,
        InventoryTransactionDto dto,
        int userId)
    {
        if (dto.Quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(dto.TransactionType))
        {
            throw new ArgumentException("Transaction type is required.");
        }

        var transactionType = dto.TransactionType.Trim().ToUpperInvariant();

        if (transactionType != "ADD" && transactionType != "SUBTRACT")
        {
            throw new ArgumentException(
                "Transaction type must be 'ADD' or 'SUBTRACT'.");
        }

        var inventory = await _inventoryRepository.GetByIdAsync(inventoryId);

        if (inventory == null)
        {
            throw new ArgumentException("Inventory does not exist.");
        }

        int quantityChange;

        if (transactionType == "ADD")
        {
            quantityChange = dto.Quantity;
            inventory.Quantity += dto.Quantity;
        }
        else
        {
            quantityChange = -dto.Quantity;

            if (inventory.Quantity < dto.Quantity)
            {
                throw new ArgumentException("Insufficient inventory.");
            }

            inventory.Quantity -= dto.Quantity;
        }

        var transaction = new InventoryTransaction
        {
            InventoryID = inventory.InventoryID,
            QuantityChange = quantityChange,
            TransactionType = transactionType,
            Reason = dto.Reason,
            TransactionDate = DateTime.UtcNow,
            UserID = userId
        };

        await _inventoryRepository.AddTransactionAsync(transaction);

        return new InventoryTransactionResponseDto
        {
            TransactionID = transaction.TransactionID,
            InventoryID = inventory.InventoryID,
            QuantityChange = quantityChange,
            TransactionType = transactionType,
            Reason = transaction.Reason,
            TransactionDate = transaction.TransactionDate,
            UserID = userId,
            CurrentQuantity = inventory.Quantity
        };
    }
}
