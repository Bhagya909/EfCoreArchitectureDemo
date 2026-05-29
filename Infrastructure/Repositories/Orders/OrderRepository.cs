using Application.Common;
using Application.DTOs.Orders;
using Application.Interfaces.Repositories;
using Application.Models;
using Domain.Entities.Orders;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Orders
{
    public class OrderRepository : IOrderRepository
    {
        private readonly RetailDbContext _context;

        private static readonly
            Func<RetailDbContext, int, IAsyncEnumerable<Order>>
            _getByIdWithItemsCompiledQuery =
                EF.CompileAsyncQuery(
                    (RetailDbContext context, int id) =>
                        context.Orders
                            .AsNoTracking()
                            .Include(o => o.OrderItems)
                                .ThenInclude(i => i.Product)
                            .AsSplitQuery()
                            .Where(o => o.Id == id));

        private static readonly
            Func<RetailDbContext, int, IAsyncEnumerable<Order>>
            _getByCustomerIdCompiledQuery =
                EF.CompileAsyncQuery(
                    (RetailDbContext context, int customerId) =>
                        context.Orders
                            .AsNoTracking()
                            .Where(o => o.CustomerId == customerId)
                            .OrderByDescending(o => o.CreatedAt)
                            .Select(o => o));

        public OrderRepository(RetailDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .AsSplitQuery()
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order?> GetByIdWithItemsAsync(int id)
        {
            await foreach (var order in
                _getByIdWithItemsCompiledQuery(_context, id))
            {
                return order;
            }

            return null;
        }

        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }

        public async Task<List<Order>> GetByCustomerIdAsync(
            int customerId)
        {
            var orders = new List<Order>();

            await foreach (var order in
                _getByCustomerIdCompiledQuery(
                    _context, customerId))
            {
                orders.Add(order);
            }

            return orders;
        }

        public async Task<PagedResult<OrderResponseDto>> GetPagedAsync(
            OrderQueryParameters parameters)
        {
            var query = _context.Orders
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(parameters.Status)
                && Enum.TryParse<OrderStatus>(
                    parameters.Status,
                    ignoreCase: true,
                    out var parsedStatus))
            {
                query = query.Where(
                    o => o.Status == parsedStatus);
            }

            if (parameters.CustomerId.HasValue)
            {
                query = query.Where(
                    o => o.CustomerId == parameters.CustomerId.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(o => new OrderResponseDto
                {
                    OrderId = o.Id,
                    CustomerId = o.CustomerId,
                    Status = o.Status.ToString(),
                    TotalAmount = o.TotalAmount,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt
                })
                .ToListAsync();

            return new PagedResult<OrderResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.Page,
                PageSize = parameters.PageSize
            };
        }
    }
}
