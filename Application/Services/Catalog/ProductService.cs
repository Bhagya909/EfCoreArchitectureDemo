using Application.Common;
using Application.DTOs.Products;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Application.Models;
using Application.Validators;
using Domain.Entities.Catalog;

namespace Application.Services.Catalog
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

            var existingProductId =
                await _productRepository.GetIdBySkuAsync(dto.SKU);

            if (existingProductId.HasValue)
                throw new InvalidOperationException(
                    $"Product with SKU '{dto.SKU}' already exists.");

            var product = new Product(dto.Name, dto.SKU, dto.BasePrice);

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

        public async Task<PagedResult<ProductResponseDto>> GetAllProductsAsync(
            ProductQueryParameters queryParameters)
        {
            return await _productRepository.GetPagedProjectedAsync(queryParameters);
        }

        public async Task<ProductResponseDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdReadOnlyAsync(id);

            if (product is null)
                return null;

            return product.ToResponseDto();
        }

        public async Task<ProductResponseDto?> UpdateProductAsync(
            int id, UpdateProductDto dto)
        {
            ProductValidator.ValidateUpdateProduct(dto);

            if (dto.RowVersion is null || dto.RowVersion.Length == 0)
                throw new ArgumentException(
                    "RowVersion is required for update.");

            var product = await _productRepository.GetByIdAsync(id);

            if (product is null)
                return null;

            if (product.IsDeleted)
                throw new InvalidOperationException(
                    "Archived products cannot be updated.");

            if (product.SKU != dto.SKU)
            {
                var existingWithSku =
                    await _productRepository.GetIdBySkuAsync(dto.SKU);

                if (existingWithSku.HasValue && existingWithSku.Value != id)
                    throw new InvalidOperationException(
                        $"Product with SKU '{dto.SKU}' already exists.");
            }

            product.SetRowVersion(dto.RowVersion);
            product.UpdateDetails(dto.Name, dto.SKU, dto.BasePrice);

            await _changeLogService.LogAsync(
                actionType: "PRODUCT_UPDATED",
                entityName: "Product",
                referenceId: product.Id,
                description:
                    $"Product '{product.Name}' updated with SKU {product.SKU} " +
                    $"and price {product.BasePrice}.",
                rawData:
                    $"Name={dto.Name}; SKU={dto.SKU}; Price={dto.BasePrice}",
                requestAiSummary: true);

            await _unitOfWork.SaveChangesAsync();

            return product.ToResponseDto();
        }

        public async Task<bool> SoftDeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product is null)
                return false;

            product.SoftDelete();

            await _changeLogService.LogAsync(
                actionType: "PRODUCT_DELETED",
                entityName: "Product",
                referenceId: product.Id,
                description:
                    $"Product '{product.Name}' with SKU {product.SKU} was soft deleted.");

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<int> BulkUpdatePricesAsync(BulkPriceUpdateDto dto)
        {
            if (dto.PercentageChange == 0)
                throw new ArgumentException(
                    "PercentageChange cannot be zero.");

            if (dto.PercentageChange < -100)
                throw new ArgumentException(
                    "PercentageChange cannot reduce prices below zero.");

            if (dto.PercentageChange > 200)
                throw new ArgumentException(
                    "PercentageChange exceeds allowed operational limit of 200%.");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var affectedRows = await _productRepository
                    .BulkUpdatePricesAsync(dto.PercentageChange, dto.CategoryId);

                var scope = dto.CategoryId.HasValue
                    ? $"CategoryId={dto.CategoryId}"
                    : "all categories";

                await _changeLogService.LogAsync(
                    actionType: "BULK_PRICE_UPDATE",
                    entityName: "Product",
                    referenceId: null,
                    description:
                        $"{affectedRows} products updated through bulk price operation.",
                    rawData:
                        $"PercentageChange={dto.PercentageChange}; Scope={scope}",
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

        public async Task<int> BulkArchiveProductsAsync(BulkArchiveProductsDto dto)
        {
            if (dto.MaxPrice < 0)
                throw new ArgumentException("MaxPrice cannot be negative.");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var affectedRows = await _productRepository
                    .BulkArchiveProductsAsync(dto.MaxPrice, dto.CategoryId);

                var scope = dto.CategoryId.HasValue
                    ? $"CategoryId={dto.CategoryId}"
                    : "all categories";

                await _changeLogService.LogAsync(
                    actionType: "BULK_PRODUCT_ARCHIVE",
                    entityName: "Product",
                    referenceId: null,
                    description:
                        $"{affectedRows} products archived through bulk operation.",
                    rawData:
                        $"MaxPrice={dto.MaxPrice}; Scope={scope}",
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

        public async Task<int> BulkRestoreProductsAsync(BulkRestoreProductsDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var affectedRows = await _productRepository
                    .BulkRestoreProductsAsync(dto.CategoryId);

                var scope = dto.CategoryId.HasValue
                    ? $"CategoryId={dto.CategoryId}"
                    : "all categories";

                await _changeLogService.LogAsync(
                    actionType: "BULK_RESTORE",
                    entityName: "Product",
                    referenceId: null,
                    description:
                        $"{affectedRows} products restored through bulk operation.",
                    rawData: $"Scope={scope}");

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
    }
}
