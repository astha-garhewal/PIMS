using PIMS.Application.DTOs.Inventory;

namespace PIMS.Application.Interfaces;

public interface IInventoryService
{
    Task<InventoryResponseDto> CreateAsync(CreateInventoryDto dto);

    Task<InventoryResponseDto?> GetByIdAsync(int inventoryId);

    Task<InventoryTransactionResponseDto> ProcessTransactionAsync(
        int inventoryId,
        InventoryTransactionDto dto,
        int userId);

    Task<InventoryAuditResponseDto> PerformAuditAsync(
        int inventoryId,
        InventoryAuditDto dto,
        int userId);

    Task<List<InventoryAuditResponseDto>> GetAuditsAsync(
        int inventoryId);
}
