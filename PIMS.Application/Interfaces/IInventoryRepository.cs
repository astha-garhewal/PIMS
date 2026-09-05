using PIMS.Domain.Entities;

namespace PIMS.Application.Interfaces;

public interface IInventoryRepository
{
    Task<Inventory?> GetByIdAsync(int inventoryId);

    Task<Inventory?> GetByProductIdAsync(int productId);

    Task AddAsync(Inventory inventory);

    Task AddTransactionAsync(InventoryTransaction transaction);

    Task AddAuditAsync(InventoryAudit audit);

    Task<List<InventoryAudit>> GetAuditsAsync(int inventoryId);
}
