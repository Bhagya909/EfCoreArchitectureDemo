using Application.DTOs.Inventory;
using Application.Interfaces.Repositories;
using Domain.Entities.Inventory;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

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

    private static readonly
        Func<RetailDbContext, int, IAsyncEnumerable<Inventory>>
        _getByProductIdReadOnlyCompiledQuery =
            EF.CompileAsyncQuery(
                (RetailDbContext context, int productId) =>
                    context.Inventories
                        .AsNoTracking()
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
                _context, productId))
        {
            return inventory;
        }

        return null;
    }

    public async Task<Inventory?> GetByProductIdReadOnlyAsync(
        int productId)
    {
        await foreach (var inventory in
            _getByProductIdReadOnlyCompiledQuery(
                _context, productId))
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

    public async Task<List<InventoryTransactionResponseDto>>
        GetTransactionsByProductIdAsync(
            int productId,
            int top = 50)
    {
        return await _context.InventoryTransactions
            .AsNoTracking()
            .Include(t => t.Product)
            .AsSplitQuery()
            .Where(t => t.ProductId == productId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(top)
            .Select(t => new InventoryTransactionResponseDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                ProductName = t.Product != null
                    ? t.Product.Name
                    : "Unknown",
                SKU = t.Product != null
                    ? t.Product.SKU
                    : "Unknown",
                QuantityChange = t.QuantityChange,
                TransactionType = t.TransactionType.ToString(),
                Reason = t.Reason,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<InventoryResponseDto>> GetLowStockAsync(
        int threshold)
    {
        return await _context.Inventories
            .AsNoTracking()
            .Include(i => i.Product)
            .AsSplitQuery()
            .Where(i => i.Quantity > 0
                && i.Quantity <= threshold)
            .OrderBy(i => i.Quantity)
            .Select(i => new InventoryResponseDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product != null
                    ? i.Product.Name
                    : "Unknown",
                SKU = i.Product != null
                    ? i.Product.SKU
                    : "Unknown",
                Quantity = i.Quantity,
                LastUpdated = i.LastUpdated
            })
            .ToListAsync();
    }

    public async Task<List<InventoryResponseDto>> GetOutOfStockAsync()
    {
        return await _context.Inventories
            .AsNoTracking()
            .Include(i => i.Product)
            .AsSplitQuery()
            .Where(i => i.Quantity == 0)
            .Select(i => new InventoryResponseDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product != null
                    ? i.Product.Name
                    : "Unknown",
                SKU = i.Product != null
                    ? i.Product.SKU
                    : "Unknown",
                Quantity = i.Quantity,
                LastUpdated = i.LastUpdated
            })
            .ToListAsync();
    }

    public async Task<InventorySummaryDto> GetSummaryAsync(
        int lowStockThreshold)
    {
        return new InventorySummaryDto
        {
            TotalProductsTracked =
                await _context.Inventories.CountAsync(),
            TotalUnitsInStock =
                await _context.Inventories
                    .SumAsync(i => i.Quantity),
            OutOfStockCount =
                await _context.Inventories
                    .CountAsync(i => i.Quantity == 0),
            LowStockCount =
                await _context.Inventories
                    .CountAsync(i =>
                        i.Quantity > 0 &&
                        i.Quantity <= lowStockThreshold),
            LowStockThreshold = lowStockThreshold
        };
    }
}
