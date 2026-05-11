using Application.DTOs.Payments;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> CompletePaymentAsync(
        int orderId);

        Task<PaymentResponseDto> FailPaymentAsync(
            int orderId);
    }
}
