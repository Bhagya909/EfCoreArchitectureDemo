using Domain.Entities.Orders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int id);

        Task AddAsync(Order order);

    }
}
