using PIMS.Application.DTOs.Categories;
using PIMS.Application.Interfaces;
using PIMS.Domain.Entities;

namespace PIMS.Application.Services;

public class CategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CategoryName))
        {
            throw new ArgumentException("Category name is required.");
        }

        var category = new Category
        {
            CategoryName = dto.CategoryName,
            Description = dto.Description
        };

        await _categoryRepository.AddAsync(category);

        return new CategoryResponseDto
        {
            CategoryID = category.CategoryID,
            CategoryName = category.CategoryName,
            Description = category.Description
        };
    }

    public async Task<List<CategoryResponseDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();

        return categories.Select(c => new CategoryResponseDto
        {
            CategoryID = c.CategoryID,
            CategoryName = c.CategoryName,
            Description = c.Description
        }).ToList();
    }
}
