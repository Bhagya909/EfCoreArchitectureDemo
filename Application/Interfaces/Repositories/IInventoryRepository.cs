using Domain.Entities.Inventory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories
{
    public interface IInventoryRepository
    {
        Task<Inventory?> GetByProductIdAsync(int productId);

        Task AddAsync(Inventory inventory);

        Task AddTransactionAsync(
            InventoryTransaction transaction);

        Task SaveChangesAsync();
    }
}
