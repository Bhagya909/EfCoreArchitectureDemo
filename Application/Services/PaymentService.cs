using Application.DTOs.Payments;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(
            IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<PaymentResponseDto>
            CompletePaymentAsync(int orderId)
        {
            var payment =
                await _paymentRepository
                    .GetByOrderIdAsync(orderId);

            if (payment is null)
            {
                throw new Exception("Payment not found.");
            }

            payment.MarkAsCompleted();

            return payment.ToResponseDto();
        }

        public async Task<PaymentResponseDto>
            FailPaymentAsync(int orderId)
        {
            var payment =
                await _paymentRepository
                    .GetByOrderIdAsync(orderId);

            if (payment is null)
            {
                throw new Exception("Payment not found.");
            }

            payment.MarkAsFailed();


            return payment.ToResponseDto();
        }
    }
}
