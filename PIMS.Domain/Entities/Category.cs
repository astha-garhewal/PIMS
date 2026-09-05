namespace PIMS.Domain.Entities;

public class Category
{
    public int CategoryID { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<ProductCategory> ProductCategories { get; set; }
        = new List<ProductCategory>();
}
