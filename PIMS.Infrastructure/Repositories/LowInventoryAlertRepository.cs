using Microsoft.EntityFrameworkCore;
using PIMS.Application.Interfaces;
using PIMS.Domain.Entities;
using PIMS.Infrastructure.Data;

namespace PIMS.Infrastructure.Repositories;

public class LowInventoryAlertRepository : ILowInventoryAlertRepository
{
    private readonly ApplicationDbContext _context;

    public LowInventoryAlertRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LowInventoryAlert?> GetActiveAlertAsync(int inventoryId)
    {
        return await _context.LowInventoryAlerts
            .FirstOrDefaultAsync(a =>
                a.InventoryID == inventoryId &&
                !a.IsResolved);
    }

    public async Task<List<LowInventoryAlert>> GetActiveAlertsAsync()
    {
        return await _context.LowInventoryAlerts
            .Include(a => a.Inventory)
            .ThenInclude(i => i.Product)
            .Where(a => !a.IsResolved)
            .OrderByDescending(a => a.AlertDate)
            .ToListAsync();
    }

    public async Task AddAsync(LowInventoryAlert alert)
    {
        await _context.LowInventoryAlerts.AddAsync(alert);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(LowInventoryAlert alert)
    {
        _context.LowInventoryAlerts.Update(alert);
        await _context.SaveChangesAsync();
    }
}
