namespace PIMS.Application.DTOs.Products;

public class BulkPriceAdjustmentDto
{
    public List<int> ProductIds { get; set; } = new();

    public decimal Value { get; set; }

    public string AdjustmentType { get; set; } = string.Empty;
}
