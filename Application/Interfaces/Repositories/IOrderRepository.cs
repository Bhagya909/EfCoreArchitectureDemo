using Application.Common;
using Application.DTOs.Orders;
using Application.Models;
using Domain.Entities.Orders;

namespace Application.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int id);
        Task<Order?> GetByIdWithItemsAsync(int id);
        Task AddAsync(Order order);
        Task<List<Order>> GetByCustomerIdAsync(int customerId);
        Task<PagedResult<OrderResponseDto>> GetPagedAsync(
            OrderQueryParameters parameters);
    }
}
