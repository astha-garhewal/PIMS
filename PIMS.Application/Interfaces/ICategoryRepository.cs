using PIMS.Domain.Entities;

namespace PIMS.Application.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int categoryId);

    Task<List<Category>> GetAllAsync();

    Task AddAsync(Category category);
}
