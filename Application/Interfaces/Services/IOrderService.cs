using Application.DTOs.Orders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(
        CreateOrderDto dto);

        Task<OrderResponseDto?> GetOrderByIdAsync(int id);
    }
}
