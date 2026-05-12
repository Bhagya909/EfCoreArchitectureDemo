using Domain.Entities.Orders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByOrderIdAsync(int orderId);

        Task AddAsync(Payment payment);

    }
}
