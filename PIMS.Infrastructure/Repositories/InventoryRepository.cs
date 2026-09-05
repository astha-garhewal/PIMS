using Microsoft.EntityFrameworkCore;
using PIMS.Application.Interfaces;
using PIMS.Domain.Entities;
using PIMS.Infrastructure.Data;

namespace PIMS.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly ApplicationDbContext _context;

    public InventoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Inventory?> GetByIdAsync(int inventoryId)
    {
        return await _context.Inventory
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.InventoryID == inventoryId);
    }

    public async Task<Inventory?> GetByProductIdAsync(int productId)
    {
        return await _context.Inventory
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.ProductID == productId);
    }

    public async Task AddAsync(Inventory inventory)
    {
        await _context.Inventory.AddAsync(inventory);
        await _context.SaveChangesAsync();
    }

    public async Task AddTransactionAsync(InventoryTransaction transaction)
    {
        await _context.InventoryTransactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task AddAuditAsync(InventoryAudit audit)
    {
        await _context.InventoryAudits.AddAsync(audit);
        await _context.SaveChangesAsync();
    }

    public async Task<List<InventoryAudit>> GetAuditsAsync(int inventoryId)
    {
        return await _context.InventoryAudits
            .Where(a => a.InventoryID == inventoryId)
            .OrderByDescending(a => a.AuditDate)
            .ToListAsync();
    }
}
