using Application.DTOs.Payments;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Domain.Entities.Orders;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;

        private readonly IOrderRepository _orderRepository;

        private readonly IInventoryService _inventoryService;

        private readonly IUnitOfWork _unitOfWork;

        private readonly IChangeLogService _changeLogService;

        private const int MaxRetryAttempts = 3;

        public PaymentService(
    IPaymentRepository paymentRepository,
    IOrderRepository orderRepository,
    IInventoryService inventoryService,
    IUnitOfWork unitOfWork,
    IChangeLogService changeLogService)
        {
            _paymentRepository = paymentRepository;

            _orderRepository = orderRepository;

            _inventoryService = inventoryService;

            _unitOfWork = unitOfWork;

            _changeLogService = changeLogService;
        }

    public async Task<PaymentResponseDto>
    CompletePaymentAsync(int orderId)
        {
            var retryCount = 0;

            while (true)
            {
                try
                {
                    await _unitOfWork
                        .BeginTransactionAsync();

                    var order =
                        await _orderRepository
                            .GetByIdAsync(orderId);

                    if (order is null)
                    {
                        throw new Exception(
                            "Order not found.");
                    }

                    if (order.Status !=
                        OrderStatus.PendingPayment)
                    {
                        throw new Exception(
                            "Order is not awaiting payment.");
                    }

                    foreach (var item in order.OrderItems)
                    {
                        await _inventoryService
                            .DeductStockAsync(
                                item.ProductId,
                                item.Quantity);
                    }

                    var payment =
                        new Payment(
                            orderId,
                            order.TotalAmount);

                    payment.MarkAsCompleted();

                    order.MarkAsPaid();

                    await _paymentRepository
                        .AddAsync(payment);

                    await _changeLogService.LogAsync(
                        "Payment_Completed",
                        "Payment",
                        payment.Id,
                        $"OrderId={order.Id}",
                        $"Payment completed for Order {order.Id}");

                    await _unitOfWork
                        .SaveChangesAsync();

                    await _unitOfWork
                        .CommitTransactionAsync();

                    return new PaymentResponseDto
                    {
                        PaymentId = payment.Id,
                        Status = payment.Status.ToString(),
                        Amount = payment.Amount,
                        PaidAt = payment.PaidAt
                    };
                }
                catch (Exception ex)
                {
                    await _unitOfWork
                        .RollbackTransactionAsync();

                    retryCount++;

                    var isConcurrencyConflict =
                        ex.Message.Contains(
                            "concurrency",
                            StringComparison.OrdinalIgnoreCase);

                    if (!isConcurrencyConflict ||
                        retryCount >= MaxRetryAttempts)
                    {
                        throw;
                    }
                }
            }
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
