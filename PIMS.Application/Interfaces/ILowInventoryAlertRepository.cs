using PIMS.Domain.Entities;

namespace PIMS.Application.Interfaces;

public interface ILowInventoryAlertRepository
{
    Task<LowInventoryAlert?> GetActiveAlertAsync(int inventoryId);

    Task<List<LowInventoryAlert>> GetActiveAlertsAsync();

    Task AddAsync(LowInventoryAlert alert);

    Task UpdateAsync(LowInventoryAlert alert);
}
