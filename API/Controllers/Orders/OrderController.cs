using Application.DTOs.Orders;
using Application.Interfaces.Services;

using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Orders
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(
            IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<ActionResult<
    OrderResponseDto>>
    CreateOrder(
        CreateOrderDto dto)
        {
            var order =
                await _orderService
                    .CreateOrderAsync(dto);

            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = order.OrderId },
                order);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<
    OrderResponseDto>>
    GetOrderById(int id)
        {
            var order =
                await _orderService
                    .GetOrderByIdAsync(id);

            if (order is null)
            {
                return NotFound();
            }

            return Ok(order);
        }


    }
}
