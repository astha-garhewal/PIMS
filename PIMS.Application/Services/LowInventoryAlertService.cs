using PIMS.Application.DTOs.Inventory;
using PIMS.Application.Interfaces;
using PIMS.Domain.Entities;

namespace PIMS.Application.Services;

public class LowInventoryAlertService
{
    private readonly ILowInventoryAlertRepository _alertRepository;

    public LowInventoryAlertService(
        ILowInventoryAlertRepository alertRepository)
    {
        _alertRepository = alertRepository;
    }

    public async Task CheckAndCreateAlertAsync(Inventory inventory)
    {
        if (inventory.Quantity > inventory.LowStockThreshold)
        {
            return;
        }

        var existingAlert =
            await _alertRepository.GetActiveAlertAsync(
                inventory.InventoryID);

        if (existingAlert != null)
        {
            return;
        }

        var alert = new LowInventoryAlert
        {
            InventoryID = inventory.InventoryID,
            CurrentQuantity = inventory.Quantity,
            Threshold = inventory.LowStockThreshold,
            AlertDate = DateTime.UtcNow,
            IsResolved = false
        };

        await _alertRepository.AddAsync(alert);
    }

    public async Task ResolveAlertIfStockRecoveredAsync(
        Inventory inventory)
    {
        if (inventory.Quantity <= inventory.LowStockThreshold)
        {
            return;
        }

        var existingAlert =
            await _alertRepository.GetActiveAlertAsync(
                inventory.InventoryID);

        if (existingAlert == null)
        {
            return;
        }

        existingAlert.IsResolved = true;

        await _alertRepository.UpdateAsync(existingAlert);
    }

    public async Task<List<LowInventoryAlertResponseDto>> GetActiveAlertsAsync()
    {
        var alerts = await _alertRepository.GetActiveAlertsAsync();

        return alerts.Select(a => new LowInventoryAlertResponseDto
        {
            AlertID = a.AlertID,
            InventoryID = a.InventoryID,
            ProductID = a.Inventory.ProductID,
            SKU = a.Inventory.Product.SKU,
            ProductName = a.Inventory.Product.Name,
            CurrentQuantity = a.CurrentQuantity,
            Threshold = a.Threshold,
            AlertDate = a.AlertDate,
            IsResolved = a.IsResolved
        }).ToList();
    }
}
