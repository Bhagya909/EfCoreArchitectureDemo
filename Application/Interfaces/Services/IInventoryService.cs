using Application.DTOs.Inventory;

namespace Application.Interfaces.Services;

public interface IInventoryService
{
    Task<InventoryResponseDto> AddInventoryAsync(
        CreateInventoryDto dto);

    Task<bool> ValidateStockAsync(
        int productId,
        int quantity);

    Task DeductStockAsync(
        int productId,
        int quantity);

    Task<InventoryResponseDto?> GetInventoryByProductIdAsync(
        int productId);

    Task<List<InventoryTransactionResponseDto>>
        GetTransactionsByProductIdAsync(int productId, int top = 50);

    Task<List<InventoryResponseDto>> GetLowStockAsync(
        int threshold);

    Task<List<InventoryResponseDto>> GetOutOfStockAsync();

    Task<InventorySummaryDto> GetSummaryAsync(
        int lowStockThreshold = 10);
}