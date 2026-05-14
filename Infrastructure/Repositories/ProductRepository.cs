using Application.Interfaces.Repositories;
using Application.Models;
using Domain.Entities.Catalog;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly RetailDbContext _context;

        public ProductRepository(RetailDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetBySkuAsync(string sku)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.SKU == sku);
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<(IEnumerable<Product> Items, int TotalCount)>
    GetPagedAsync(
        ProductQueryParameters queryParameters)
        {
            var query = _context.Products
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(
                queryParameters.SearchTerm))
            {
                query = query.Where(p =>
                    p.Name.Contains(
                        queryParameters.SearchTerm) ||
                    p.SKU.Contains(
                        queryParameters.SearchTerm));
            }

            query = queryParameters.SortBy?.ToLower() switch
            {
                "name" => query.OrderBy(p => p.Name),

                "price" => query.OrderBy(p => p.BasePrice),

                _ => query.OrderBy(p => p.Id)
            };

            var totalCount =
                await query.CountAsync();

            var items = await query
                .Skip(
                    (queryParameters.PageNumber - 1)
                    * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

     
    }
}
