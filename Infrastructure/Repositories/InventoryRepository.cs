using Domain.Entities.Inventory;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Repositories;

namespace Infrastructure.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly RetailDbContext _context;

        private static readonly
            Func<RetailDbContext, int, IAsyncEnumerable<Inventory>>
            _getByProductIdCompiledQuery =
                EF.CompileAsyncQuery(
                    (RetailDbContext context, int productId) =>
                        context.Inventories
                            .Where(i => i.ProductId == productId));

        public InventoryRepository(RetailDbContext context)
        {
            _context = context;
        }

        public async Task<Inventory?> GetByProductIdAsync(
            int productId)
        {
            await foreach (var inventory in
                _getByProductIdCompiledQuery(
                    _context,
                    productId))
            {
                return inventory;
            }

            return null;
        }

        public async Task AddAsync(Inventory inventory)
        {
            await _context.Inventories.AddAsync(inventory);
        }

        public async Task AddTransactionAsync(
            InventoryTransaction transaction)
        {
            await _context.InventoryTransactions
                .AddAsync(transaction);
        }
    }
}