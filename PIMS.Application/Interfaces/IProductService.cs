using PIMS.Application.DTOs.Products;

namespace PIMS.Application.Interfaces;

public interface IProductService
{
    Task<ProductResponseDto> CreateAsync(CreateProductDto dto);

    Task<List<ProductResponseDto>> GetAllAsync();

    Task<ProductResponseDto?> GetByIdAsync(int productId);

    Task<List<ProductResponseDto>> SearchAsync(
        string? search,
        int? categoryId);

    Task<ProductResponseDto?> AdjustPriceAsync(
        int productId,
        PriceAdjustmentDto dto);

    Task<List<ProductResponseDto>> BulkAdjustPriceAsync(
        BulkPriceAdjustmentDto dto);
}
