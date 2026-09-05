namespace PIMS.Domain.Entities;

public class InventoryTransaction
{
    public int TransactionID { get; set; }

    public int InventoryID { get; set; }

    public int QuantityChange { get; set; }

    public string TransactionType { get; set; } = string.Empty;

    public string? Reason { get; set; }

    public DateTime TransactionDate { get; set; }

    public int UserID { get; set; }

    public Inventory Inventory { get; set; } = null!;

    public User User { get; set; } = null!;
}
