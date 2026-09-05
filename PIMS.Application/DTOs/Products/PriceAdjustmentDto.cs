namespace PIMS.Application.DTOs.Products;

public class PriceAdjustmentDto
{
    public decimal Value { get; set; }

    public string AdjustmentType { get; set; } = string.Empty;
}
