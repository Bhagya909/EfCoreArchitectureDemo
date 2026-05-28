using Application.Common;
using Application.DTOs.Orders;
using Application.Interfaces.Services;
using Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Orders
{
    [ApiController]
    [Route("api/orders")]
    [Produces("application/json")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<OrderResponseDto>> CreateOrder(
            CreateOrderDto dto)
        {
            var order = await _orderService.CreateOrderAsync(dto);

            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = order.OrderId },
                order);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<OrderResponseDto>>> GetAllOrders(
            [FromQuery] OrderQueryParameters parameters)
        {
            var orders = await _orderService.GetAllOrdersAsync(parameters);

            return Ok(orders);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderResponseDto>> GetOrderById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order is null)
                return NotFound(new
                {
                    message = $"Order with id {id} was not found."
                });

            return Ok(order);
        }

        [HttpPut("{id:int}/cancel")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<OrderResponseDto>> CancelOrder(int id)
        {
            var order = await _orderService.CancelOrderAsync(id);

            if (order is null)
                return NotFound(new
                {
                    message = $"Order with id {id} was not found."
                });

            return Ok(order);
        }

        [HttpPut("{id:int}/complete")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<OrderResponseDto>> CompleteOrder(int id)
        {
            var order = await _orderService.CompleteOrderAsync(id);

            if (order is null)
                return NotFound(new
                {
                    message = $"Order with id {id} was not found."
                });

            return Ok(order);
        }
    }
}
