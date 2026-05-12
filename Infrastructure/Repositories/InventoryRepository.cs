using Domain.Entities.Inventory;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly RetailDbContext _context;

        public InventoryRepository(RetailDbContext context)
        {
            _context = context;
        }

        public async Task<Inventory?> GetByProductIdAsync(
            int productId)
        {
            return await _context.Inventories
                .FirstOrDefaultAsync(i =>
                    i.ProductId == productId);
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

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
