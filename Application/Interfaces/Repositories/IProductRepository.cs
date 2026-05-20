using Application.Common;
using Application.DTOs.Products;
using Application.Models;
using Domain.Entities.Catalog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetBySkuAsync(string sku);

        Task<Product?> GetByIdAsync(int id);

        Task<PagedResult<Product>>
    GetPagedAsync(
        ProductQueryParameters queryParameters);

        Task AddAsync(Product product);

        Task<PagedResult<ProductResponseDto>>
    GetPagedProjectedAsync(
        ProductQueryParameters queryParameters);

     Task<int> BulkIncreasePricesAsync(
        decimal percentageIncrease);

     Task<int> BulkArchiveProductsAsync(
            decimal maxPrice);

    }
}
