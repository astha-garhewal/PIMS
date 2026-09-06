using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PIMS.Application.DTOs.Products;
using PIMS.Application.Interfaces;

namespace PIMS.API.Controllers;

[ApiController]
[Route("api/v1/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? categoryId)
    {
        var products = await _productService.SearchAsync(
            search,
            categoryId);

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        var product = await _productService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.ProductID },
            product);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Update(
        int id,
        UpdateProductDto dto)
    {
        var product = await _productService.UpdateAsync(id, dto);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPut("price/bulk")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> BulkAdjustPrice(
        BulkPriceAdjustmentDto dto)
    {
        var products = await _productService.BulkAdjustPriceAsync(dto);

        return Ok(products);
    }

    [HttpPut("{id:int}/price")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> AdjustPrice(
        int id,
        PriceAdjustmentDto dto)
    {
        var product = await _productService.AdjustPriceAsync(id, dto);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }
}
