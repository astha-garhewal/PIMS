namespace PIMS.Application.DTOs.Categories;

public class CategoryResponseDto
{
    public int CategoryID { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }
}
