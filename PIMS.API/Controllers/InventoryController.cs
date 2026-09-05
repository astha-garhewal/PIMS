using Microsoft.AspNetCore.Mvc;
using PIMS.Application.DTOs.Inventory;
using PIMS.Application.Interfaces;

namespace PIMS.API.Controllers;

[ApiController]
[Route("api/v1/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateInventoryDto dto)
    {
        var inventory = await _inventoryService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = inventory.InventoryID },
            inventory);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var inventory = await _inventoryService.GetByIdAsync(id);

        if (inventory == null)
        {
            return NotFound();
        }

        return Ok(inventory);
    }

    [HttpPost("{id:int}/transactions")]
    public async Task<IActionResult> ProcessTransaction(
        int id,
        InventoryTransactionDto dto)
    {
        var userId = 1;

        var transaction = await _inventoryService
            .ProcessTransactionAsync(id, dto, userId);

        return Ok(transaction);
    }
}
