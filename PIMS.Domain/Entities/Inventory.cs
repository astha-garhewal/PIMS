namespace PIMS.Domain.Entities;

public class Inventory
{
    public int InventoryID { get; set; }

    public int ProductID { get; set; }

    public int Quantity { get; set; }

    public string WarehouseLocation { get; set; } = string.Empty;

    public int LowStockThreshold { get; set; }

    public Product Product { get; set; } = null!;

    public ICollection<InventoryTransaction> Transactions { get; set; }
        = new List<InventoryTransaction>();

    public ICollection<InventoryAudit> Audits { get; set; }
        = new List<InventoryAudit>();

    public ICollection<LowInventoryAlert> LowInventoryAlerts { get; set; }
        = new List<LowInventoryAlert>();
}
