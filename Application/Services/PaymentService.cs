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
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order =
                    await _orderRepository
                        .GetByIdAsync(orderId);

                if (order is null)
                {
                    throw new Exception(
                        "Order not found.");
                }

                if (order.Status != OrderStatus.PendingPayment)
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

                await _changeLogService.LogAsync(
                "INVENTORY_DEDUCTED",
                "Inventory",
                null,
                $"OrderId={order.Id}",
                $"Inventory deducted for Order {order.Id}.");

                var payment = new Payment(
                    orderId,
                    order.TotalAmount);

                payment.MarkAsCompleted();

                order.MarkAsPaid();

                await _paymentRepository
                    .AddAsync(payment);

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                await _changeLogService.LogAsync(
                "PAYMENT_COMPLETED",
                "Payment",
                payment.Id,
                $"OrderId={order.Id}",
                $"Payment completed for Order {order.Id}.");

                await _changeLogService.LogAsync(
                "ORDER_PAID",
                "Order",
                order.Id,
                null,
                $"Order {order.Id} marked as paid");

                return new PaymentResponseDto
                {
                    PaymentId = payment.Id,
                    Status = payment.Status.ToString(),
                    Amount = payment.Amount,
                    PaidAt = payment.PaidAt
                };
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();

                throw;
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
