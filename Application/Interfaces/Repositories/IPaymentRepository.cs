using Application.DTOs.Payments;
using Domain.Entities.Orders;

namespace Application.Interfaces.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(int id);
        Task<Payment?> GetByOrderIdAsync(int orderId);
        Task<PaymentResponseDto?> GetByIdReadOnlyAsync(int id);
        Task<PaymentResponseDto?> GetByOrderIdReadOnlyAsync(int orderId);
        Task AddAsync(Payment payment);
    }
}