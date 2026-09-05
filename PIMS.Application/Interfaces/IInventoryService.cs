using PIMS.Application.DTOs.Inventory;

namespace PIMS.Application.Interfaces;

public interface IInventoryService
{
    Task<InventoryResponseDto> CreateAsync(CreateInventoryDto dto);

    Task<InventoryResponseDto?> GetByIdAsync(int inventoryId);
}
