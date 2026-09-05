using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PIMS.Application.DTOs.Inventory;
using PIMS.Application.Interfaces;
using PIMS.Application.Services;

namespace PIMS.API.Controllers;

[ApiController]
[Route("api/v1/inventory")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly LowInventoryAlertService _alertService;

    public InventoryController(
        IInventoryService inventoryService,
        LowInventoryAlertService alertService)
    {
        _inventoryService = inventoryService;
        _alertService = alertService;
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

    [HttpGet("alerts")]
    public async Task<IActionResult> GetActiveAlerts()
    {
        var alerts = await _alertService.GetActiveAlertsAsync();

        return Ok(alerts);
    }

    [HttpPost("{id:int}/audits")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> PerformAudit(
        int id,
        InventoryAuditDto dto)
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var audit = await _inventoryService
            .PerformAuditAsync(id, dto, userId);

        return Ok(audit);
    }

    [HttpGet("{id:int}/audits")]
    public async Task<IActionResult> GetAudits(int id)
    {
        var audits = await _inventoryService.GetAuditsAsync(id);

        return Ok(audits);
    }

    [HttpPost("{id:int}/transactions")]
    [Authorize]
    public async Task<IActionResult> ProcessTransaction(
        int id,
        InventoryTransactionDto dto)
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var transaction = await _inventoryService
            .ProcessTransactionAsync(id, dto, userId);

        return Ok(transaction);
    }
}
