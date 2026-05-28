using Application.DTOs.Payments;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Payments
{
    [ApiController]
    [Route("api/payments")]
    [Produces("application/json")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<PaymentResponseDto>> CompletePayment(
            CompletePaymentDto dto)
        {
            var payment = await _paymentService
                .CompletePaymentAsync(dto.OrderId);

            return CreatedAtAction(
                nameof(GetPaymentById),
                new { id = payment.PaymentId },
                payment);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentResponseDto>> GetPaymentById(
            int id)
        {
            var payment = await _paymentService
                .GetPaymentByIdAsync(id);

            if (payment is null)
                return NotFound(new
                {
                    message = $"Payment with id {id} was not found."
                });

            return Ok(payment);
        }

        [HttpGet("order/{orderId:int}")]
        [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentResponseDto>> GetPaymentByOrderId(
            int orderId)
        {
            var payment = await _paymentService
                .GetPaymentByOrderIdAsync(orderId);

            if (payment is null)
                return NotFound(new
                {
                    message = $"Payment for order {orderId} was not found."
                });

            return Ok(payment);
        }
    }
}