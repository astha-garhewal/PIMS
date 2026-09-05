namespace PIMS.Domain.Entities;

public class LowInventoryAlert
{
    public int AlertID { get; set; }

    public int InventoryID { get; set; }

    public int CurrentQuantity { get; set; }

    public int Threshold { get; set; }

    public DateTime AlertDate { get; set; }

    public bool IsResolved { get; set; }

    public Inventory Inventory { get; set; } = null!;
}
