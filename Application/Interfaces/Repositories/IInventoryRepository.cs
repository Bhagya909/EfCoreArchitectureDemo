using Application.DTOs.Inventory;
using Domain.Entities.Inventory;

namespace Application.Interfaces.Repositories;

public interface IInventoryRepository
{
    Task<Inventory?> GetByProductIdAsync(int productId);

    Task<Inventory?> GetByProductIdReadOnlyAsync(int productId);

    Task AddAsync(Inventory inventory);

    Task AddTransactionAsync(
        InventoryTransaction transaction);

    Task<List<InventoryTransactionResponseDto>>
        GetTransactionsByProductIdAsync(
            int productId,
            int top = 50);

    Task<List<InventoryResponseDto>> GetLowStockAsync(int threshold);

    Task<List<InventoryResponseDto>> GetOutOfStockAsync();

    Task<InventorySummaryDto> GetSummaryAsync(
        int lowStockThreshold);
}
