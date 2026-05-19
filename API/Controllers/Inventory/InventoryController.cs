using Application.DTOs.Inventory;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Inventory
{
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(
            IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }


        [HttpPost("add")]
        public async Task<ActionResult<
            InventoryResponseDto>>
            AddInventory(
                CreateInventoryDto dto)
        {
            var inventory =
                await _inventoryService
                    .AddInventoryAsync(dto);

            return Ok(inventory);
        }


        [HttpGet("validate")]
        public async Task<ActionResult<bool>>
            ValidateStock(
                int productId,
                int quantity)
        {
            var isAvailable =
                await _inventoryService
                    .ValidateStockAsync(
                        productId,
                        quantity);

            return Ok(isAvailable);
        }
    }
}
