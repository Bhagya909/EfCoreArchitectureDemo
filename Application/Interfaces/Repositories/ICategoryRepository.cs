using Application.Common;
using Application.DTOs.Categories;
using Domain.Entities.Catalog;

namespace Application.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id);
        Task<CategoryDetailResponseDto?> GetByIdWithProductCountAsync(int id);
        Task<bool> ExistsByNameAsync(string name);
        Task<List<CategoryResponseDto>> GetAllAsync();
        Task<PagedResult<ProductCategoryResponseDto>> GetProductsByCategoryAsync(
            int categoryId, int page, int pageSize);
        Task AddAsync(Category category);
        Task<bool> ExistsByIdAsync(int id);
        Task<int> ArchiveProductsByCategoryAsync(int categoryId);

        Task<List<ProductCategoryResponseDto>> GetCategoriesForProductAsync(
    int productId);
    }
}