using Application.Common;
using Application.DTOs.Orders;
using Domain.Entities.Orders;

namespace Application.Mappings
{
    public static class OrderMappings
    {
        public static OrderItemResponseDto ToResponseDto(
            this OrderItem item)
        {
            return new OrderItemResponseDto
            {
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? string.Empty,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.Quantity * item.UnitPrice
            };
        }

        public static OrderResponseDto ToResponseDto(
            this Order order)
        {
            return new OrderResponseDto
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order.OrderItems
                    .Select(i => i.ToResponseDto())
                    .ToList()
            };
        }
    }
}