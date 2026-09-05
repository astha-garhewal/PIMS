namespace PIMS.Domain.Entities;

public class ProductCategory
{
    public int ProductID { get; set; }

    public int CategoryID { get; set; }

    public Product Product { get; set; } = null!;

    public Category Category { get; set; } = null!;
}
