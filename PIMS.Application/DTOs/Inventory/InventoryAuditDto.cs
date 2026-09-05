namespace PIMS.Application.DTOs.Inventory;

public class InventoryAuditDto
{
    public int AdjustedQuantity { get; set; }

    public string Reason { get; set; } = string.Empty;
}
