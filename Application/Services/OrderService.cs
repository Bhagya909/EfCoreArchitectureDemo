using Application.Common;
using Application.DTOs.Orders;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Application.Models;
using Application.Validators;
using Domain.Entities.Orders;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IChangeLogService _changeLogService;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IPaymentRepository paymentRepository,
            IInventoryService inventoryService,
            IUnitOfWork unitOfWork,
            IChangeLogService changeLogService)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _paymentRepository = paymentRepository;
            _inventoryService = inventoryService;
            _unitOfWork = unitOfWork;
            _changeLogService = changeLogService;
        }

        public async Task<OrderResponseDto> CreateOrderAsync(
            CreateOrderDto dto)
        {
            OrderValidator.ValidateCreateOrder(dto);

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in dto.Items)
            {
                var product = await _productRepository
                    .GetByIdAsync(item.ProductId);

                if (product is null)
                    throw new KeyNotFoundException(
                        $"Product {item.ProductId} not found.");

                var stockAvailable = await _inventoryService
                    .ValidateStockAsync(item.ProductId, item.Quantity);

                if (!stockAvailable)
                    throw new InvalidOperationException(
                        $"Insufficient stock for product '{product.Name}'.");

                totalAmount += product.BasePrice * item.Quantity;

                orderItems.Add(new OrderItem(
                    product.Id,
                    item.Quantity,
                    product.BasePrice));
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = new Order(dto.CustomerId, totalAmount);

                foreach (var item in orderItems)
                    order.AddOrderItem(item);

                await _orderRepository.AddAsync(order);

                await _changeLogService.LogAsync(
                    actionType: "ORDER_CREATED",
                    entityName: "Order",
                    referenceId: order.Id,
                    description:
                        $"Order created for CustomerId " +
                        $"{dto.CustomerId} with total {totalAmount}.",
                    rawData:
                        $"CustomerId={dto.CustomerId}; " +
                        $"Total={totalAmount}; " +
                        $"Items={dto.Items.Count}");

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return order.ToResponseDto();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<OrderResponseDto?> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository
                .GetByIdWithItemsAsync(id);

            if (order is null)
                return null;

            return order.ToResponseDto();
        }

        public async Task<PagedResult<OrderResponseDto>> GetAllOrdersAsync(
            OrderQueryParameters parameters)
        {
            return await _orderRepository.GetPagedAsync(parameters);
        }

        public async Task<OrderResponseDto?> CancelOrderAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);

            if (order is null)
                return null;

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                order.Cancel();

                await _changeLogService.LogAsync(
                    actionType: "ORDER_CANCELLED",
                    entityName: "Order",
                    referenceId: order.Id,
                    description:
                        $"Order {order.Id} cancelled " +
                        $"for CustomerId {order.CustomerId}.",
                    rawData:
                        $"OrderId={order.Id}; " +
                        $"CustomerId={order.CustomerId}");

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return order.ToResponseDto();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<OrderResponseDto?> CompleteOrderAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);

            if (order is null)
                return null;

            var payment = await _paymentRepository.GetByOrderIdAsync(id);

            if (payment is null ||
                payment.Status != Domain.Enums.PaymentStatus.Completed)
            {
                throw new InvalidOperationException(
                    "Order can only be completed after payment is completed.");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                order.MarkAsCompleted();

                await _changeLogService.LogAsync(
                    actionType: "ORDER_COMPLETED",
                    entityName: "Order",
                    referenceId: order.Id,
                    description:
                        $"Order {order.Id} completed after successful payment.",
                    rawData:
                        $"OrderId={order.Id}; " +
                        $"PaymentId={payment.Id}");

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return order.ToResponseDto();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
