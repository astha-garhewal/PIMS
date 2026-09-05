namespace PIMS.Application.DTOs.Inventory;

public class LowInventoryAlertResponseDto
{
    public int AlertID { get; set; }

    public int InventoryID { get; set; }

    public int ProductID { get; set; }

    public string SKU { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public int CurrentQuantity { get; set; }

    public int Threshold { get; set; }

    public DateTime AlertDate { get; set; }

    public bool IsResolved { get; set; }
}
