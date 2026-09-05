namespace PIMS.Application.DTOs.Inventory;

public class InventoryAuditResponseDto
{
    public int AuditID { get; set; }

    public int InventoryID { get; set; }

    public int PreviousQuantity { get; set; }

    public int AdjustedQuantity { get; set; }

    public int Difference { get; set; }

    public string Reason { get; set; } = string.Empty;

    public DateTime AuditDate { get; set; }

    public int UserID { get; set; }
}
