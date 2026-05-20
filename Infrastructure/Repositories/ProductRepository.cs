using Application.Common;
using Application.DTOs.Products;
using Application.Interfaces.Repositories;
using Application.Models;
using Domain.Entities.Catalog;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly RetailDbContext _context;

        // Compiled Query
        private static readonly
            Func<RetailDbContext, string, IAsyncEnumerable<Product>>
            _getBySkuCompiledQuery =
                EF.CompileAsyncQuery(
                    (RetailDbContext context, string sku) =>
                        context.Products
                            .AsNoTracking()
                            .Where(p => p.SKU == sku));

        public ProductRepository(RetailDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetBySkuAsync(string sku)
        {
            await foreach (var product in
                _getBySkuCompiledQuery(_context, sku))
            {
                return product;
            }

            return null;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<PagedResult<Product>>
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

            return new PagedResult<Product>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = queryParameters.PageNumber,
                PageSize = queryParameters.PageSize
            };
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

        public async Task<
            PagedResult<ProductResponseDto>>
            GetPagedProjectedAsync(
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

                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    BasePrice = p.BasePrice
                })

                .ToListAsync();

            return new PagedResult<ProductResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = queryParameters.PageNumber,
                PageSize = queryParameters.PageSize
            };
        }
        public async Task<int> BulkIncreasePricesAsync(
            decimal percentageIncrease)
        {
            var multiplier =
                1 + (percentageIncrease / 100);

            return await _context.Products
                .Where(p => !p.IsDeleted)
                .ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(
                            p => p.BasePrice,
                            p => p.BasePrice * multiplier)

                        .SetProperty(
                            p => p.UpdatedAt,
                            DateTime.UtcNow));
        }

        public async Task<int> BulkArchiveProductsAsync(
            decimal maxPrice)
        {
            return await _context.Products
                .Where(p =>
                    !p.IsDeleted &&
                    p.BasePrice <= maxPrice)
                .ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(
                            p => p.IsDeleted,
                            true)

                        .SetProperty(
                            p => p.UpdatedAt,
                            DateTime.UtcNow));
        }
    }
}