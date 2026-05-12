using Application.DTOs.Payments;
using Domain.Entities.Orders;
using System;
using System.Collections.Generic;
using System.Text;

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
                Status = payment.Status.ToString(),
                Amount = payment.Amount,
                PaidAt = payment.PaidAt
            };
        }
    }
}
