using Application.DTOs.Orders;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Orders;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        private readonly IProductRepository _productRepository;

        private readonly IInventoryService _inventoryService;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IInventoryService inventoryService)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _inventoryService = inventoryService;
        }

        public async Task<OrderResponseDto>
            CreateOrderAsync(CreateOrderDto dto)
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

            foreach (var item in dto.Items)
            {
                await _inventoryService.DeductStockAsync(
                    item.ProductId,
                    item.Quantity);
            }

            await _orderRepository.SaveChangesAsync();

            return new OrderResponseDto
            {
                OrderId = order.Id,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt
            };
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

            return new OrderResponseDto
            {
                OrderId = order.Id,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt
            };
        }

    }
}
