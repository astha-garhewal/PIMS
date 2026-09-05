namespace PIMS.Application.DTOs.Inventory;

public class InventoryResponseDto
{
    public int InventoryID { get; set; }

    public int ProductID { get; set; }

    public string SKU { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string WarehouseLocation { get; set; } = string.Empty;

    public int LowStockThreshold { get; set; }
}
