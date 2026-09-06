namespace PIMS.Application.DTOs.Products;

public class UpdateProductDto
{
    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }
}
