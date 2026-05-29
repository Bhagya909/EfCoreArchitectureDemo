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
    [Tags("Retail Workflow - Orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Creates a PendingPayment order after validating requested products and available stock.
        /// </summary>
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

        /// <summary>
        /// Lists orders with pagination and query filters for operations review.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<OrderResponseDto>>> GetAllOrders(
            [FromQuery] OrderQueryParameters parameters)
        {
            var orders = await _orderService.GetAllOrdersAsync(parameters);

            return Ok(orders);
        }

        /// <summary>
        /// Gets an order with its line items and current lifecycle status.
        /// </summary>
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

        /// <summary>
        /// Cancels an order that is still waiting for payment.
        /// </summary>
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

        /// <summary>
        /// Completes a paid order and prevents completion before successful payment.
        /// </summary>
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
