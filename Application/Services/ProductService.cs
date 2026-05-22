using Application.Common;
using Application.DTOs.Products;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Application.Models;
using Application.Validators;
using Domain.Entities.Catalog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        private readonly IChangeLogService _changeLogService;

        private readonly IUnitOfWork _unitOfWork;

        public ProductService(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IChangeLogService changeLogService)
        {
            _productRepository = productRepository;

            _unitOfWork = unitOfWork;

            _changeLogService = changeLogService;
        }

        public async Task<ProductResponseDto> CreateProductAsync(
            CreateProductDto dto)
        {
            ProductValidator.ValidateCreateProduct(dto);
            var existingProduct =
                await _productRepository.GetBySkuAsync(dto.SKU);

            if (existingProduct is not null)
            {
                throw new Exception(
                    $"Product with SKU '{dto.SKU}' already exists.");
            }

            var product = new Product(
                dto.Name,
                dto.SKU,
                dto.BasePrice);

            await _productRepository.AddAsync(product);
            await _changeLogService.LogAsync(
                actionType: "PRODUCT_CREATED",
                entityName: "Product",
                referenceId: product.Id,
                description: $"Product '{product.Name}' created with SKU {product.SKU}.",
                rawData: $"SKU={product.SKU}; Price={product.BasePrice}");

            await _unitOfWork.SaveChangesAsync();

            return product.ToResponseDto();

        }

        public async Task<PagedResult<ProductResponseDto>>
    GetAllProductsAsync(
        ProductQueryParameters queryParameters)
        {
            return await _productRepository
                .GetPagedProjectedAsync(
                    queryParameters);
        }

        public async Task<ProductResponseDto?> GetProductByIdAsync(
            int id)
        {
            var product =
                await _productRepository.GetByIdAsync(id);

            if (product is null)
            {
                return null;
            }

            return product.ToResponseDto();
        }

        public async Task<bool> SoftDeleteProductAsync(int id)
        {
            var product =
                await _productRepository.GetByIdAsync(id);

            if (product is null)
            {
                return false;
            }

            product.SoftDelete();
            await _changeLogService.LogAsync(
                actionType: "PRODUCT_DELETED",
                entityName: "Product",
                referenceId: product.Id,
                description:
                    $"Product '{product.Name}' " +
                    $"with SKU {product.SKU} was soft deleted.");
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        public async Task<int> BulkIncreasePricesAsync(
    BulkPriceUpdateDto dto)
        {
            if (dto.PercentageIncrease <= 0)
            {
                throw new Exception(
                    "Percentage increase must be greater than zero.");
            }

            if (dto.PercentageIncrease > 200)
            {
                throw new Exception(
                    "Percentage increase exceeds allowed operational limit.");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var affectedRows =
                    await _productRepository
                        .BulkIncreasePricesAsync(
                            dto.PercentageIncrease);

                await _changeLogService.LogAsync(
                    actionType: "BULK_PRICE_UPDATE",
                    entityName: "Product",
                    referenceId: null,
                    description: $"{affectedRows} products updated through bulk price operation.",
                    rawData: $"PercentageIncrease={dto.PercentageIncrease}",
                    requestAiSummary: true
                    );

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                return affectedRows;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();

                throw;
            }
        }
        public async Task<int> BulkArchiveProductsAsync(
    BulkArchiveProductsDto dto)
        {
            if (dto.MaxPrice < 0)
            {
                throw new Exception(
                    "Max price cannot be negative.");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var affectedRows =
                    await _productRepository
                        .BulkArchiveProductsAsync(
                            dto.MaxPrice);

                await _changeLogService.LogAsync(
                    actionType: "BULK_PRODUCT_ARCHIVE",
                    entityName: "Product",
                    referenceId: null,
                    description: $"{affectedRows} products archived through bulk operation.",
                    rawData: $"MaxPrice={dto.MaxPrice}",
                    requestAiSummary: true);

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                return affectedRows;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();

                throw;
            }
        }

        public async Task<ProductResponseDto?> UpdateProductAsync(
    int id,
    UpdateProductDto dto)
        {
            var product =
                await _productRepository.GetByIdAsync(id);

            if (product is null)
            {
                return null;
            }

            // Check if new SKU conflicts with another product
            if (product.SKU != dto.SKU)
            {
                var existingWithSku =
                    await _productRepository
                        .GetBySkuAsync(dto.SKU);

                if (existingWithSku is not null
                    && existingWithSku.Id != id)
                {
                    throw new InvalidOperationException(
                        $"Product with SKU '{dto.SKU}' already exists.");
                }
            }

            product.UpdateDetails(
                dto.Name,
                dto.SKU,
                dto.BasePrice);

            await _changeLogService.LogAsync(
                actionType: "PRODUCT_UPDATED",
                entityName: "Product",
                referenceId: product.Id,
                description:
                    $"Product '{product.Name}' " +
                    $"updated with SKU {product.SKU} " +
                    $"and price {product.BasePrice}.",
                rawData:
                    $"Name={dto.Name}; " +
                    $"SKU={dto.SKU}; " +
                    $"Price={dto.BasePrice}",
                requestAiSummary: true);

            await _unitOfWork.SaveChangesAsync();

            return product.ToResponseDto();
        }
    }
}
