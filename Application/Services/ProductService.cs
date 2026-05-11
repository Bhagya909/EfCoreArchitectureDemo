using Application.DTOs.Products;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Catalog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(
            IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductResponseDto> CreateProductAsync(
            CreateProductDto dto)
        {
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

            await _productRepository.SaveChangesAsync();

            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                BasePrice = product.BasePrice
            };
        }

        public async Task<IEnumerable<ProductResponseDto>>
            GetAllProductsAsync()
        {
            var products =
                await _productRepository.GetAllAsync();

            return products.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                BasePrice = p.BasePrice
            });
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

            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                BasePrice = product.BasePrice
            };
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

            await _productRepository.SaveChangesAsync();

            return true;
        }
    }
}
