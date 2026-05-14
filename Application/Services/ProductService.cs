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
            "CREATE",
            "Product",
            product.Id,
            $"SKU={product.SKU}",
            $"Product '{product.Name}' created.");

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

            return true;
        }
    }
}
