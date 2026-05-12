using Application.DTOs.Orders;
using Domain.Entities.Orders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Mappings
{
    public static class OrderMappings
    {
        public static OrderResponseDto ToResponseDto(
            this Order order)
        {
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
