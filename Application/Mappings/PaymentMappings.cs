using Application.DTOs.Payments;
using Domain.Entities.Orders;

namespace Application.Mappings
{
    public static class PaymentMappings
    {
        public static PaymentResponseDto ToResponseDto(
            this Payment payment)
        {
            return new PaymentResponseDto
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                Status = payment.Status.ToString(),
                Amount = payment.Amount,
                PaidAt = payment.PaidAt,
                ReferenceNumber = payment.ReferenceNumber
            };
        }
    }
}