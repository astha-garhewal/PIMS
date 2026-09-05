using PIMS.Application.DTOs.Products;

namespace PIMS.Application.Interfaces;

public interface IProductService
{
    Task<ProductResponseDto> CreateAsync(CreateProductDto dto);

    Task<List<ProductResponseDto>> GetAllAsync();

    Task<ProductResponseDto?> GetByIdAsync(int productId);
}
