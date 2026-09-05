namespace PIMS.Application.DTOs.Products;

public class ProductResponseDto
{
    public int ProductID { get; set; }

    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public DateTime CreatedDate { get; set; }

    public List<string> Categories { get; set; } = new();
}
