using Application.DTOs.Payments;
using Application.Interfaces.Services;

using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(
            IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("complete/{orderId:int}")]
        public async Task<ActionResult<
    PaymentResponseDto>>
    CompletePayment(int orderId)
        {
            var payment =
                await _paymentService
                    .CompletePaymentAsync(orderId);

            return Ok(payment);
        }

        [HttpPost("fail/{orderId:int}")]
        public async Task<ActionResult<
    PaymentResponseDto>>
    FailPayment(int orderId)
        {
            var payment =
                await _paymentService
                    .FailPaymentAsync(orderId);

            return Ok(payment);
        }
    }
}
