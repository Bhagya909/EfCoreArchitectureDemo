using Application.Common;
using Application.DTOs.Products;
using Application.Interfaces.Repositories;
using Application.Models;
using Domain.Entities.Catalog;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Catalog
{
    public class ProductRepository : IProductRepository
    {
        private readonly RetailDbContext _context;

        private static readonly
            Func<RetailDbContext, string, IAsyncEnumerable<int?>>
            _getIdBySkuCompiledQuery =
                EF.CompileAsyncQuery(
                    (RetailDbContext context, string sku) =>
                        context.Products
                            .AsNoTracking()
                            .Where(p => p.SKU == sku)
                            .Select(p => (int?)p.Id));

        public ProductRepository(RetailDbContext context)
        {
            _context = context;
        }

        public async Task<int?> GetIdBySkuAsync(string sku)
        {
            await foreach (var productId in
                _getIdBySkuCompiledQuery(_context, sku))
            {
                return productId;
            }

            return null;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> GetByIdWithCategoriesAsync(int id)
        {
            return await _context.Products
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> GetByIdReadOnlyAsync(int id)
        {
            return await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

        public async Task<PagedResult<ProductResponseDto>> GetPagedProjectedAsync(
            ProductQueryParameters queryParameters)
        {
            var query = _context.Products
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(queryParameters.SearchTerm))
            {
                query = query.Where(p =>
                    p.Name.Contains(queryParameters.SearchTerm) ||
                    p.SKU.Contains(queryParameters.SearchTerm));
            }

            query = queryParameters.SortBy?.ToLower() switch
            {
                "name" => query.OrderBy(p => p.Name),
                "price" => query.OrderBy(p => p.BasePrice),
                _ => query.OrderBy(p => p.Id)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    BasePrice = p.BasePrice,
                    RowVersion = p.RowVersion
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

        public async Task<int> BulkUpdatePricesAsync(
            decimal percentageChange, int? categoryId)
        {
            var multiplier = 1 + (percentageChange / 100);

            var query = _context.Products
                .Where(p => !p.IsDeleted);

            if (categoryId.HasValue)
            {
                query = query.Where(p =>
                    p.ProductCategories.Any(pc =>
                        pc.CategoryId == categoryId.Value));
            }

            return await query.ExecuteUpdateAsync(setters =>
                setters
                    .SetProperty(p => p.BasePrice, p => p.BasePrice * multiplier)
                    .SetProperty(p => p.UpdatedAt, DateTime.UtcNow));
        }

        public async Task<int> BulkArchiveProductsAsync(
            decimal maxPrice, int? categoryId)
        {
            var query = _context.Products
                .Where(p => !p.IsDeleted && p.BasePrice <= maxPrice);

            if (categoryId.HasValue)
            {
                query = query.Where(p =>
                    p.ProductCategories.Any(pc =>
                        pc.CategoryId == categoryId.Value));
            }

            return await query.ExecuteUpdateAsync(setters =>
                setters
                    .SetProperty(p => p.IsDeleted, true)
                    .SetProperty(p => p.UpdatedAt, DateTime.UtcNow));
        }

        public async Task<int> BulkRestoreProductsAsync(int? categoryId)
        {
            // Global query filter excludes IsDeleted — use IgnoreQueryFilters
            var query = _context.Products
                .IgnoreQueryFilters()
                .Where(p => p.IsDeleted);

            if (categoryId.HasValue)
            {
                query = query.Where(p =>
                    p.ProductCategories.Any(pc =>
                        pc.CategoryId == categoryId.Value));
            }

            return await query.ExecuteUpdateAsync(setters =>
                setters
                    .SetProperty(p => p.IsDeleted, false)
                    .SetProperty(p => p.UpdatedAt, DateTime.UtcNow));
        }
    }
}
