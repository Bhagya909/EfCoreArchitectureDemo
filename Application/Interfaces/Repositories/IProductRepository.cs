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

        Task<(IEnumerable<Product> Items, int TotalCount)>
    GetPagedAsync(
        ProductQueryParameters queryParameters);

        Task AddAsync(Product product);

    }
}
