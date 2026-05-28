using Application.Common;
using Application.DTOs.Categories;
using Application.Interfaces.Repositories;
using Domain.Entities.Catalog;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly RetailDbContext _context;

        private static readonly
            Func<RetailDbContext, int, IAsyncEnumerable<CategoryDetailResponseDto>>
            _getByIdWithProductCountCompiledQuery =
                EF.CompileAsyncQuery(
                    (RetailDbContext context, int id) =>
                        context.Categories
                            .AsNoTracking()
                            .Where(c => c.Id == id)
                            .Select(c => new CategoryDetailResponseDto
                            {
                                Id = c.Id,
                                Name = c.Name,
                                ProductCount = c.ProductCategories
                                    .Count(pc => !pc.Product.IsDeleted),
                                CreatedAt = c.CreatedAt,
                                UpdatedAt = c.UpdatedAt
                            }));

        private static readonly
            Func<RetailDbContext, string, IAsyncEnumerable<bool>>
            _existsByNameCompiledQuery =
                EF.CompileAsyncQuery(
                    (RetailDbContext context, string name) =>
                        context.Categories
                            .AsNoTracking()
                            .Where(c => c.Name == name)
                            .Select(c => true));

        private static readonly
            Func<RetailDbContext, int, IAsyncEnumerable<bool>>
            _existsByIdCompiledQuery =
                EF.CompileAsyncQuery(
                    (RetailDbContext context, int id) =>
                        context.Categories
                            .AsNoTracking()
                            .Where(c => c.Id == id)
                            .Select(c => true));

        public CategoryRepository(RetailDbContext context)
        {
            _context = context;
        }

        // Tracked — write path
        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // Untracked — compiled projected read
        public async Task<CategoryDetailResponseDto?> GetByIdWithProductCountAsync(
            int id)
        {
            await foreach (var category in
                _getByIdWithProductCountCompiledQuery(_context, id))
            {
                return category;
            }

            return null;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            await foreach (var _ in
                _existsByNameCompiledQuery(_context, name))
            {
                return true;
            }

            return false;
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            await foreach (var _ in
                _existsByIdCompiledQuery(_context, id))
            {
                return true;
            }

            return false;
        }

        public async Task<List<CategoryResponseDto>> GetAllAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CategoryResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<PagedResult<ProductCategoryResponseDto>>
            GetProductsByCategoryAsync(
                int categoryId,
                int page,
                int pageSize)
        {
            var query = _context.ProductCategories
                .AsNoTracking()
                .Where(pc => pc.CategoryId == categoryId)
                .Where(pc => !pc.Product.IsDeleted);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(pc => pc.Product.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(pc => new ProductCategoryResponseDto
                {
                    CategoryId = categoryId,
                    CategoryName = pc.Category.Name
                })
                .ToListAsync();

            return new PagedResult<ProductCategoryResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
        }

        public async Task<int> ArchiveProductsByCategoryAsync(
            int categoryId)
        {
            return await _context.Products
                .Where(p => !p.IsDeleted &&
                    p.ProductCategories.Any(
                        pc => pc.CategoryId == categoryId))
                .ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(p => p.IsDeleted, true)
                        .SetProperty(p => p.UpdatedAt, DateTime.UtcNow));
        }

        public async Task<List<ProductCategoryResponseDto>>
            GetCategoriesForProductAsync(int productId)
        {
            return await _context.ProductCategories
                .AsNoTracking()
                .Where(pc => pc.ProductId == productId)
                .Select(pc => new ProductCategoryResponseDto
                {
                    CategoryId = pc.CategoryId,
                    CategoryName = pc.Category.Name
                })
                .ToListAsync();
        }
    }
}
