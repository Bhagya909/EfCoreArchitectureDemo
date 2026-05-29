using Application.DTOs.Payments;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Domain.Entities.Orders;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Services.Orders
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

        public async Task<PaymentResponseDto> CompletePaymentAsync(
            int orderId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var order = await _orderRepository
                    .GetByIdAsync(orderId);

                if (order is null)
                    throw new KeyNotFoundException(
                        $"Order {orderId} not found.");

                if (order.Status != OrderStatus.PendingPayment)
                    throw new InvalidOperationException(
                        $"Order {orderId} is not awaiting payment. " +
                        $"Current status: {order.Status}.");

                foreach (var item in order.OrderItems)
                {
                    await _inventoryService.DeductStockAsync(
                        item.ProductId,
                        item.Quantity);
                }

                var payment = new Payment(orderId, order.TotalAmount);
                payment.MarkAsCompleted();
                order.MarkAsPaid();

                await _paymentRepository.AddAsync(payment);

                await _changeLogService.LogAsync(
                    actionType: "PAYMENT_COMPLETED",
                    entityName: "Payment",
                    referenceId: payment.Id,
                    description:
                        $"Payment completed for Order {orderId}. " +
                        $"Amount: {order.TotalAmount}.",
                    rawData:
                        $"OrderId={orderId}; " +
                        $"Amount={order.TotalAmount}; " +
                        $"Items={order.OrderItems.Count}",
                    requestAiSummary: true);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return payment.ToResponseDto();
            }
            catch (ConcurrencyConflictException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _unitOfWork.ClearChanges();
                await LogPaymentFailureAsync(orderId, ex);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _unitOfWork.ClearChanges();
                await LogPaymentFailureAsync(orderId, ex);
                throw;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                _unitOfWork.ClearChanges();
                throw;
            }
        }

        public async Task<PaymentResponseDto?> GetPaymentByIdAsync(int id)
        {
            return await _paymentRepository.GetByIdReadOnlyAsync(id);
        }

        public async Task<PaymentResponseDto?> GetPaymentByOrderIdAsync(
            int orderId)
        {
            return await _paymentRepository
                .GetByOrderIdReadOnlyAsync(orderId);
        }

        private async Task LogPaymentFailureAsync(
            int orderId,
            Exception exception)
        {
            await _changeLogService.LogAsync(
                actionType: "PAYMENT_FAILED",
                entityName: "Payment",
                referenceId: orderId,
                description:
                    $"Payment failed for Order {orderId}: " +
                    $"{exception.Message}",
                rawData:
                    $"OrderId={orderId}; " +
                    $"Error={exception.GetType().Name}; " +
                    $"Message={exception.Message}");

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
