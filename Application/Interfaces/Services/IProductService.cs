using Application.Common;
using Application.DTOs.Products;
using Application.Models;

namespace Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<ProductResponseDto> CreateProductAsync(CreateProductDto dto);
        Task<PagedResult<ProductResponseDto>> GetAllProductsAsync(
            ProductQueryParameters queryParameters);
        Task<ProductResponseDto?> GetProductByIdAsync(int id);
        Task<ProductResponseDto?> UpdateProductAsync(int id, UpdateProductDto dto);
        Task<bool> SoftDeleteProductAsync(int id);
        Task<int> BulkUpdatePricesAsync(BulkPriceUpdateDto dto);
        Task<int> BulkArchiveProductsAsync(BulkArchiveProductsDto dto);
        Task<int> BulkRestoreProductsAsync(BulkRestoreProductsDto dto);
    }
}