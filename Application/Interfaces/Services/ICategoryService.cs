using Application.Common;
using Application.DTOs.Categories;

namespace Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<CategoryResponseDto> CreateCategoryAsync(
            CreateCategoryDto dto);
        Task<List<CategoryResponseDto>> GetAllCategoriesAsync();
        Task<CategoryDetailResponseDto?> GetCategoryByIdAsync(int id);
        Task<CategoryResponseDto?> UpdateCategoryAsync(
            int id, UpdateCategoryDto dto);
        Task<bool> SoftDeleteCategoryAsync(int id);
        Task<PagedResult<ProductCategoryResponseDto>> GetProductsByCategoryAsync(
            int categoryId, int page, int pageSize);
        Task<int> ArchiveProductsByCategoryAsync(int categoryId);
        Task<CategoryResponseDto?> AssignCategoryToProductAsync(
            int productId, AssignCategoryDto dto);
        Task<bool> RemoveCategoryFromProductAsync(
            int productId, int categoryId);
        Task<List<ProductCategoryResponseDto>> GetCategoriesForProductAsync(
            int productId);
    }
}