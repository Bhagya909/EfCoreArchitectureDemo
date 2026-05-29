using Application.DTOs.Payments;
using Application.Interfaces.Repositories;
using Domain.Entities.Orders;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Orders
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly RetailDbContext _context;

        private static readonly
            Func<RetailDbContext, int, IAsyncEnumerable<PaymentResponseDto>>
            _getByOrderIdReadOnlyCompiledQuery =
                EF.CompileAsyncQuery(
                    (RetailDbContext context, int orderId) =>
                        context.Payments
                            .AsNoTracking()
                            .Where(p => p.OrderId == orderId)
                            .Select(p => new PaymentResponseDto
                            {
                                PaymentId = p.Id,
                                OrderId = p.OrderId,
                                Status = p.Status.ToString(),
                                Amount = p.Amount,
                                PaidAt = p.PaidAt,
                                ReferenceNumber = p.ReferenceNumber
                            }));

        private static readonly
            Func<RetailDbContext, int, IAsyncEnumerable<PaymentResponseDto>>
            _getByIdReadOnlyCompiledQuery =
                EF.CompileAsyncQuery(
                    (RetailDbContext context, int id) =>
                        context.Payments
                            .AsNoTracking()
                            .Where(p => p.Id == id)
                            .Select(p => new PaymentResponseDto
                            {
                                PaymentId = p.Id,
                                OrderId = p.OrderId,
                                Status = p.Status.ToString(),
                                Amount = p.Amount,
                                PaidAt = p.PaidAt,
                                ReferenceNumber = p.ReferenceNumber
                            }));

        public PaymentRepository(RetailDbContext context)
        {
            _context = context;
        }

        // Tracked — write path only
        public async Task<Payment?> GetByOrderIdAsync(int orderId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        // Tracked — write path only
        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // Untracked — projected, compiled
        public async Task<PaymentResponseDto?> GetByOrderIdReadOnlyAsync(
            int orderId)
        {
            await foreach (var payment in
                _getByOrderIdReadOnlyCompiledQuery(
                    _context, orderId))
            {
                return payment;
            }

            return null;
        }

        // Untracked — projected, compiled
        public async Task<PaymentResponseDto?> GetByIdReadOnlyAsync(
            int id)
        {
            await foreach (var payment in
                _getByIdReadOnlyCompiledQuery(
                    _context, id))
            {
                return payment;
            }

            return null;
        }

        public async Task AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
        }
    }
}