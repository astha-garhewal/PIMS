namespace PIMS.Domain.Entities;

public class Product
{
    public int ProductID { get; set; }

    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public ICollection<ProductCategory> ProductCategories { get; set; }
        = new List<ProductCategory>();
}