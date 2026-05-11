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

        Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync();

        Task<ProductResponseDto?> GetProductByIdAsync(int id);

        Task<bool> SoftDeleteProductAsync(int id);
    }
}
