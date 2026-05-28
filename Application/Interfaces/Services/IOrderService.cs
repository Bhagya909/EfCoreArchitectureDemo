using Application.Common;
using Application.DTOs.Orders;
using Application.Models;

namespace Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(
            CreateOrderDto dto);
        Task<OrderResponseDto?> GetOrderByIdAsync(int id);
        Task<PagedResult<OrderResponseDto>> GetAllOrdersAsync(
            OrderQueryParameters parameters);
        Task<OrderResponseDto?> CancelOrderAsync(int id);
        Task<OrderResponseDto?> CompleteOrderAsync(int id);
    }
}
