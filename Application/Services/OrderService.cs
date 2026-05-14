using Application.DTOs.Orders;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Application.Validators;
using Domain.Entities.Orders;


namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IOrderRepository _orderRepository;

        private readonly IProductRepository _productRepository;

        private readonly IInventoryService _inventoryService;

        private readonly IChangeLogService _changeLogService;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IInventoryService inventoryService,
            IUnitOfWork unitOfWork,
            IChangeLogService changeLogService)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _inventoryService = inventoryService;
            _unitOfWork = unitOfWork;
            _changeLogService = changeLogService;
        }

       

        public async Task<OrderResponseDto>
    CreateOrderAsync(CreateOrderDto dto)
        {
            OrderValidator.ValidateCreateOrder(dto);
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                decimal totalAmount = 0;

                var orderItems = new List<OrderItem>();

                foreach (var item in dto.Items)
                {
                    var product =
                        await _productRepository
                            .GetByIdAsync(item.ProductId);

                    if (product is null)
                    {
                        throw new Exception(
                            $"Product {item.ProductId} not found.");
                    }

                    var stockAvailable =
                        await _inventoryService.ValidateStockAsync(
                            item.ProductId,
                            item.Quantity);

                    if (!stockAvailable)
                    {
                        throw new Exception(
                            $"Insufficient stock for product {product.Name}");
                    }

                    totalAmount +=
                        product.BasePrice * item.Quantity;

                    orderItems.Add(new OrderItem(
                        product.Id,
                        item.Quantity,
                        product.BasePrice));
                }

                var order = new Order(
                    dto.CustomerId,
                    totalAmount);

                foreach (var item in orderItems)
                {
                    order.AddOrderItem(item);
                }

                await _orderRepository.AddAsync(order);

                await _changeLogService.LogAsync(
                "ORDER_CREATED",
                "Order",
                order.Id,
                $"CustomerId={dto.CustomerId}",
                $"Order created with total {totalAmount}");

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

        public async Task<OrderResponseDto?> GetOrderByIdAsync(
            int id)
        {
            var order =
                await _orderRepository.GetByIdAsync(id);

            if (order is null)
            {
                return null;
            }

            return order.ToResponseDto();
        }

    }
}
