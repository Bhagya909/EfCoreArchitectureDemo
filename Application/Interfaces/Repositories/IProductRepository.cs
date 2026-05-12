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
        int pageNumber,
        int pageSize,
        string? searchTerm);

        Task AddAsync(Product product);

    }
}
