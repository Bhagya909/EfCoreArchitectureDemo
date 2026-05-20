using Application.DTOs.Inventory;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Validators;
using Domain.Entities.Inventory;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
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

        public async Task<InventoryResponseDto>
            AddInventoryAsync(CreateInventoryDto dto)
        {
            var product = await _productRepository
                .GetByIdAsync(dto.ProductId);

            if (product is null || product.IsDeleted)
                throw new InvalidOperationException(
                    "Cannot add inventory for an archived product.");
            InventoryValidator.ValidateInventory(dto);
            var inventory =
                await _inventoryRepository
                    .GetByProductIdAsync(dto.ProductId);

            if (inventory is null)
            {
                inventory = new Inventory(
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
                referenceId: inventory.Id,
                description: $"Inventory updated for ProductId {dto.ProductId}.",
                rawData: $"Quantity={dto.Quantity}");



            await _unitOfWork.SaveChangesAsync();



            return new InventoryResponseDto
            {
                ProductId = inventory.ProductId,
                Quantity = inventory.Quantity,
                LastUpdated = inventory.LastUpdated
            };
        }

        public async Task<bool> ValidateStockAsync(
            int productId,
            int quantity)
        {
            var inventory =
                await _inventoryRepository
                    .GetByProductIdAsync(productId);

            return inventory is not null
                && inventory.Quantity >= quantity;
        }

        public async Task DeductStockAsync(
    int productId,
    int quantity)
        {
            var inventory =
                await _inventoryRepository
                    .GetByProductIdAsync(productId);

            if (inventory is null)
            {
                throw new Exception(
                    "Inventory not found.");
            }

            inventory.RemoveStock(quantity);

            var transaction = new InventoryTransaction(
                productId,
                quantity,
                InventoryTransactionType.OUT,
                "Stock Deducted");

            await _inventoryRepository
                .AddTransactionAsync(transaction);

            await _changeLogService.LogAsync(
                actionType: "INVENTORY_DEDUCTED",
                entityName: "Inventory",
                referenceId: inventory.Id,
                description: $"Inventory deducted for ProductId {productId}.",
                rawData: $"ProductId={productId}; Quantity={quantity}");


        }
    }
}
