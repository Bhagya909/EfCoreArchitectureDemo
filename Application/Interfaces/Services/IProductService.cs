using Application.Common;
using Application.DTOs.Products;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<ProductResponseDto> CreateProductAsync(
        CreateProductDto dto);

        Task<PagedResult<ProductResponseDto>>
    GetAllProductsAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm);

        Task<ProductResponseDto?> GetProductByIdAsync(int id);

        Task<bool> SoftDeleteProductAsync(int id);
    }
}
