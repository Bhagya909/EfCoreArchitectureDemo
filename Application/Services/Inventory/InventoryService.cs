using Application.DTOs.Inventory;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Application.Validators;
using Domain.Entities.Inventory;
using Domain.Enums;
using InventoryEntity = Domain.Entities.Inventory.Inventory;

namespace Application.Services.Inventory;

public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IChangeLogService _changeLogService;
    private readonly IProductRepository _productRepository;

    public InventoryService(
        IInventoryRepository inventoryRepository,
        IUnitOfWork unitOfWork,
        IChangeLogService changeLogService,
        IProductRepository productRepository)
    {
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
        _changeLogService = changeLogService;
        _productRepository = productRepository;
    }

    public async Task<InventoryResponseDto> AddInventoryAsync(
        CreateInventoryDto dto)
    {
        // FIX 3 — validate DTO before any DB call
        InventoryValidator.ValidateInventory(dto);

        var product = await _productRepository
            .GetByIdAsync(dto.ProductId);

        if (product is null || product.IsDeleted)
            throw new InvalidOperationException(
                "Cannot add inventory for an archived product.");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var inventory = await _inventoryRepository
                .GetByProductIdAsync(dto.ProductId);

            if (inventory is null)
            {
                inventory = new InventoryEntity(
                    dto.ProductId,
                    dto.Quantity);

                await _inventoryRepository.AddAsync(inventory);
            }
            else
            {
                inventory.AddStock(dto.Quantity);
            }

            var transaction = new InventoryTransaction(
                dto.ProductId,
                dto.Quantity,
                InventoryTransactionType.IN,
                "Stock Added");

            await _inventoryRepository
                .AddTransactionAsync(transaction);

            await _changeLogService.LogAsync(
                actionType: "INVENTORY_ADD",
                entityName: "Inventory",
                referenceId: dto.ProductId,
                description:
                    $"Stock added for product " +
                    $"'{product.Name}'. " +
                    $"Quantity: {dto.Quantity}.",
                rawData:
                    $"ProductId={dto.ProductId}; " +
                    $"Quantity={dto.Quantity}");

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return inventory.ToResponseDto(product);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<bool> ValidateStockAsync(
        int productId,
        int quantity)
    {
        if (productId <= 0)
            throw new ArgumentException(
                "Invalid product id.");

        if (quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");

        var product = await _productRepository
            .GetByIdReadOnlyAsync(productId);

        if (product is null)
            throw new KeyNotFoundException(
                $"Product with id {productId} was not found.");

        var inventory = await _inventoryRepository
            .GetByProductIdReadOnlyAsync(productId);

        return inventory is not null
            && inventory.Quantity >= quantity;
    }

    // Intentionally no SaveChangesAsync
    // Caller (PaymentService) owns the transaction
    public async Task DeductStockAsync(
        int productId,
        int quantity)
    {
        var inventory = await _inventoryRepository
            .GetByProductIdAsync(productId);

        if (inventory is null)
            throw new InvalidOperationException(
                "Inventory not found.");

        inventory.RemoveStock(quantity);

        var transaction = new InventoryTransaction(
            productId,
            -quantity,
            InventoryTransactionType.OUT,
            "Stock Deducted");

        await _inventoryRepository
            .AddTransactionAsync(transaction);

        await _changeLogService.LogAsync(
            actionType: "INVENTORY_DEDUCTED",
            entityName: "Inventory",
            referenceId: inventory.ProductId,
            description:
                $"Inventory deducted for " +
                $"ProductId {productId}.",
            rawData:
                $"ProductId={productId}; " +
                $"Quantity={quantity}");
    }

    public async Task<InventoryResponseDto?>
        GetInventoryByProductIdAsync(int productId)
    {
        var inventory = await _inventoryRepository
            .GetByProductIdReadOnlyAsync(productId);

        if (inventory is null)
            return null;

        var product = await _productRepository
            .GetByIdReadOnlyAsync(productId);

        if (product is null)
            throw new KeyNotFoundException(
                $"Product {productId} not found.");

        return inventory.ToResponseDto(product);
    }

    // FIX 2 — pass top to repository
    public async Task<List<InventoryTransactionResponseDto>>
        GetTransactionsByProductIdAsync(
            int productId,
            int top = 50)
    {
        var product = await _productRepository
            .GetByIdReadOnlyAsync(productId);

        if (product is null)
            throw new KeyNotFoundException(
                $"Product {productId} not found.");

        var transactions = await _inventoryRepository
            .GetTransactionsByProductIdAsync(productId, top);

        return transactions;
    }

    public async Task<List<InventoryResponseDto>>
        GetLowStockAsync(int threshold)
    {
        var inventories = await _inventoryRepository
            .GetLowStockAsync(threshold);

        return inventories;
    }

    public async Task<List<InventoryResponseDto>>
        GetOutOfStockAsync()
    {
        var inventories = await _inventoryRepository
            .GetOutOfStockAsync();

        return inventories;
    }

    public async Task<InventorySummaryDto> GetSummaryAsync(
        int lowStockThreshold = 10)
    {
        return await _inventoryRepository
            .GetSummaryAsync(lowStockThreshold);
    }
}
