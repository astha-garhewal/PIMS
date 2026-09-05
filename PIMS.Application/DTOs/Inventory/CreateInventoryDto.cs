namespace PIMS.Application.DTOs.Inventory;

public class CreateInventoryDto
{
    public int ProductID { get; set; }

    public int Quantity { get; set; }

    public string WarehouseLocation { get; set; } = string.Empty;

    public int LowStockThreshold { get; set; }
}
