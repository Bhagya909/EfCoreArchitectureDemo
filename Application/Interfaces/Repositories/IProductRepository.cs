using Application.Common;
using Application.DTOs.Products;
using Application.Models;
using Domain.Entities.Catalog;

namespace Application.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<int?> GetIdBySkuAsync(string sku);
        Task<Product?> GetByIdAsync(int id);
        Task<Product?> GetByIdWithCategoriesAsync(int id);
        Task<Product?> GetByIdReadOnlyAsync(int id);
        Task AddAsync(Product product);
        Task<PagedResult<ProductResponseDto>> GetPagedProjectedAsync(
            ProductQueryParameters queryParameters);
        Task<int> BulkUpdatePricesAsync(decimal percentageChange, int? categoryId);
        Task<int> BulkArchiveProductsAsync(decimal maxPrice, int? categoryId);
        Task<int> BulkRestoreProductsAsync(int? categoryId);
    }
}
