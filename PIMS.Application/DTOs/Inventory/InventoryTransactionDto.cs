namespace PIMS.Application.DTOs.Inventory;

public class InventoryTransactionDto
{
    public int Quantity { get; set; }

    public string TransactionType { get; set; } = string.Empty;

    public string? Reason { get; set; }
}
