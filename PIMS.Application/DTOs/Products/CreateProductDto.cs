namespace PIMS.Application.DTOs.Products;

public class CreateProductDto
{
    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public List<int> CategoryIds { get; set; } = new();
}
