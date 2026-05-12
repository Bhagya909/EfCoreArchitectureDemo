using Application.DTOs.Payments;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
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

            await _paymentRepository.SaveChangesAsync();

            return new PaymentResponseDto
            {
                PaymentId = payment.Id,
                Status = payment.Status.ToString(),
                Amount = payment.Amount,
                PaidAt = payment.PaidAt
            };
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

            await _paymentRepository.SaveChangesAsync();

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
