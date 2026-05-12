using Application.Interfaces.Repositories;
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
        int pageNumber,
        int pageSize,
        string? searchTerm)
        {
            var query = _context.Products
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    p.SKU.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

     
    }
}
