using Application.Common;
using Application.DTOs.Inventory;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Inventory;

[ApiController]
[Route("api/inventory")]
[Produces("application/json")]
[Tags("Inventory")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(
        IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    /// <summary>
    /// Adds stock for an existing product and records the inventory movement.
    /// </summary>
    [HttpPost("add")]
    [ProducesResponseType(typeof(InventoryResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<InventoryResponseDto>>
        AddInventory(
            [FromBody] CreateInventoryDto dto)
    {
        var inventory = await _inventoryService
            .AddInventoryAsync(dto);

        return StatusCode(201, inventory);
    }

    /// <summary>
    /// Checks whether a product has enough stock for an order before checkout.
    /// </summary>
    [HttpGet("validate")]
    [ProducesResponseType(typeof(StockValidationResultDto), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> ValidateStock(
        [FromQuery] int productId,
        [FromQuery] int quantity)
    {
        if (productId <= 0)
            return BadRequest(new
            {
                message = "Invalid product id."
            });

        if (quantity <= 0)
            return BadRequest(new
            {
                message =
                    "Quantity must be greater than zero."
            });

        var isAvailable = await _inventoryService
            .ValidateStockAsync(productId, quantity);

        return Ok(new
        {
            productId,
            quantity,
            isAvailable
        });
    }

    /// <summary>
    /// Gets the current inventory position for a product.
    /// </summary>
    [HttpGet("{productId:int}")]
    [ProducesResponseType(typeof(InventoryResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<InventoryResponseDto>>
        GetByProductId(int productId)
    {
        var inventory = await _inventoryService
            .GetInventoryByProductIdAsync(productId);

        if (inventory is null)
        {
            return NotFound(new
            {
                message =
                    $"No inventory found for " +
                    $"product {productId}."
            });
        }

        return Ok(inventory);
    }

    /// <summary>
    /// Lists recent inventory transactions for a product, newest first.
    /// </summary>
    [HttpGet("transactions/{productId:int}")]
    [ProducesResponseType(
        typeof(List<InventoryTransactionResponseDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<
        List<InventoryTransactionResponseDto>>>
        GetTransactions(
            int productId,
            [FromQuery] int top = 50)
    {
        if (top <= 0)
            return BadRequest(new
            {
                message =
                    "Top must be greater than zero."
            });

        var transactions = await _inventoryService
            .GetTransactionsByProductIdAsync(productId, top);

        return Ok(transactions);
    }

    /// <summary>
    /// Lists products whose stock is at or below the selected low-stock threshold.
    /// </summary>
    [HttpGet("low-stock")]
    [ProducesResponseType(
        typeof(List<InventoryResponseDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<
        List<InventoryResponseDto>>>
        GetLowStock(
            [FromQuery] int threshold = 10)
    {
        if (threshold <= 0)
            return BadRequest(new
            {
                message =
                    "Threshold must be greater than zero."
            });

        var items = await _inventoryService
            .GetLowStockAsync(threshold);

        return Ok(items);
    }

    /// <summary>
    /// Lists tracked products that currently have zero stock.
    /// </summary>
    [HttpGet("out-of-stock")]
    [ProducesResponseType(
        typeof(List<InventoryResponseDto>), 200)]
    public async Task<ActionResult<
        List<InventoryResponseDto>>>
        GetOutOfStock()
    {
        var items = await _inventoryService
            .GetOutOfStockAsync();

        return Ok(items);
    }

    /// <summary>
    /// Returns aggregate inventory metrics for dashboard and operational review.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(InventorySummaryDto), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<InventorySummaryDto>>
        GetSummary(
            [FromQuery] int lowStockThreshold = 10)
    {
        if (lowStockThreshold <= 0)
            return BadRequest(new
            {
                message =
                    "Low stock threshold must " +
                    "be greater than zero."
            });

        var summary = await _inventoryService
            .GetSummaryAsync(lowStockThreshold);

        return Ok(summary);
    }
}
