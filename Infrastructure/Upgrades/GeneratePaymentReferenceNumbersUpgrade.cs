using Application.Interfaces.Upgrades;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Upgrades
{
    public class GeneratePaymentReferenceNumbersUpgrade
        : IDataUpgrade
    {
        private readonly RetailDbContext _context;

        public string Name =>
            "GeneratePaymentReferenceNumbers";

        public GeneratePaymentReferenceNumbersUpgrade(
            RetailDbContext context)
        {
            _context = context;
        }

        public async Task ExecuteAsync()
        {
            var payments = await _context.Payments
                .Where(p => p.ReferenceNumber == null)
                .ToListAsync();

            foreach (var payment in payments)
            {
                payment.SetReferenceNumber(
                    $"PAY-{payment.Id:D6}");
            }
        }
    }
}