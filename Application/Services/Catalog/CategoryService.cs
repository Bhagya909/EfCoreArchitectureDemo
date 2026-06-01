using Application.Common;
using Application.DTOs.Categories;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Domain.Entities.Catalog;

namespace Application.Services.Catalog
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChangeLogService _changeLogService;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IProductRepository productRepository,
            IUnitOfWork unitOfWork,
            IChangeLogService changeLogService)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _changeLogService = changeLogService;
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(
            CreateCategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException(
                    "Category name cannot be empty.");

            var nameExists = await _categoryRepository
                .ExistsByNameAsync(dto.Name.Trim());

            if (nameExists)
                throw new InvalidOperationException(
                    $"Category '{dto.Name}' already exists.");

            var category = new Category(dto.Name);

            await _categoryRepository.AddAsync(category);

            await _changeLogService.LogAsync(
                actionType: LogActionTypes.CategoryCreated,
                entityName: LogEntityNames.Category,
                referenceId: category.Id,
                description:
                    $"Category '{category.Name}' created.",
                rawData:
                    $"Name={category.Name}");

            await _unitOfWork.SaveChangesAsync();

            return category.ToResponseDto();
        }

        public async Task<List<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<CategoryDetailResponseDto?> GetCategoryByIdAsync(
            int id)
        {
            return await _categoryRepository
                .GetByIdWithProductCountAsync(id);
        }

        public async Task<CategoryResponseDto?> UpdateCategoryAsync(
            int id,
            UpdateCategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException(
                    "Category name cannot be empty.");

            var category = await _categoryRepository
                .GetByIdAsync(id);

            if (category is null)
                return null;

            var requestedName = dto.Name.Trim();

            // Check name conflict with another category
            var nameChanged = !string.Equals(
                category.Name,
                requestedName,
                StringComparison.OrdinalIgnoreCase);

            var nameExists = nameChanged &&
                await _categoryRepository
                    .ExistsByNameAsync(requestedName);

            if (nameExists)
                throw new InvalidOperationException(
                    $"Category '{dto.Name}' already exists.");

            category.UpdateName(requestedName);

            await _changeLogService.LogAsync(
                actionType: LogActionTypes.CategoryUpdated,
                entityName: LogEntityNames.Category,
                referenceId: category.Id,
                description:
                    $"Category '{category.Name}' name updated.",
                rawData:
                    $"NewName={dto.Name}");

            await _unitOfWork.SaveChangesAsync();

            return category.ToResponseDto();
        }

        public async Task<bool> SoftDeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository
                .GetByIdAsync(id);

            if (category is null)
                return false;

            category.SoftDelete();

            await _changeLogService.LogAsync(
                actionType: LogActionTypes.CategoryDeleted,
                entityName: LogEntityNames.Category,
                referenceId: category.Id,
                description:
                    $"Category '{category.Name}' was soft deleted.",
                rawData:
                    $"CategoryId={id}");

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<PagedResult<ProductCategoryResponseDto>>
            GetProductsByCategoryAsync(
                int categoryId,
                int page,
                int pageSize)
        {
            var categoryExists = await _categoryRepository
                .ExistsByIdAsync(categoryId);

            if (!categoryExists)
                throw new KeyNotFoundException(
                    $"Category {categoryId} not found.");

            return await _categoryRepository
                .GetProductsByCategoryAsync(categoryId, page, pageSize);
        }

        public async Task<int> ArchiveProductsByCategoryAsync(
            int categoryId)
        {
            var categoryExists = await _categoryRepository
                .ExistsByIdAsync(categoryId);

            if (!categoryExists)
                throw new KeyNotFoundException(
                    $"Category {categoryId} not found.");

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var affectedRows = await _categoryRepository
                    .ArchiveProductsByCategoryAsync(categoryId);

                await _changeLogService.LogAsync(
                    actionType: LogActionTypes.CategoryProductsArchived,
                    entityName: LogEntityNames.Category,
                    referenceId: categoryId,
                    description:
                        $"{affectedRows} products archived " +
                        $"for CategoryId {categoryId}.",
                    rawData:
                        $"CategoryId={categoryId}; " +
                        $"AffectedRows={affectedRows}",
                    requestAiSummary: true);

                await _unitOfWork.SaveChangesAsync();

                return affectedRows;
            });
        }

        public async Task<CategoryResponseDto?> AssignCategoryToProductAsync(
            int productId,
            AssignCategoryDto dto)
        {
            var product = await _productRepository
                .GetByIdAsync(productId);

            if (product is null)
                throw new KeyNotFoundException(
                    $"Product {productId} not found.");

            var categoryExists = await _categoryRepository
                .ExistsByIdAsync(dto.CategoryId);

            if (!categoryExists)
                throw new KeyNotFoundException(
                    $"Category {dto.CategoryId} not found.");

            var alreadyAssigned = product.ProductCategories
                .Any(pc => pc.CategoryId == dto.CategoryId);

            if (alreadyAssigned)
                throw new InvalidOperationException(
                    $"Product {productId} is already assigned " +
                    $"to Category {dto.CategoryId}.");

            var productCategory = new ProductCategory
            {
                ProductId = productId,
                CategoryId = dto.CategoryId
            };

            product.ProductCategories.Add(productCategory);

            await _changeLogService.LogAsync(
                actionType: LogActionTypes.CategoryAssigned,
                entityName: LogEntityNames.ProductCategory,
                referenceId: productId,
                description:
                    $"Category {dto.CategoryId} assigned " +
                    $"to Product {productId}.",
                rawData:
                    $"ProductId={productId}; " +
                    $"CategoryId={dto.CategoryId}");

            await _unitOfWork.SaveChangesAsync();

            var category = await _categoryRepository
                .GetByIdWithProductCountAsync(dto.CategoryId);

            return category is null ? null : new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                CreatedAt = category.CreatedAt
            };
        }

        public async Task<bool> RemoveCategoryFromProductAsync(
            int productId,
            int categoryId)
        {
            var product = await _productRepository
                .GetByIdWithCategoriesAsync(productId);

            if (product is null)
                return false;

            var productCategory = product.ProductCategories
                .FirstOrDefault(pc => pc.CategoryId == categoryId);

            if (productCategory is null)
                return false;

            product.ProductCategories.Remove(productCategory);

            await _changeLogService.LogAsync(
                actionType: LogActionTypes.CategoryRemoved,
                entityName: LogEntityNames.ProductCategory,
                referenceId: productId,
                description:
                    $"Category {categoryId} removed " +
                    $"from Product {productId}.",
                rawData:
                    $"ProductId={productId}; " +
                    $"CategoryId={categoryId}");

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<List<ProductCategoryResponseDto>>
            GetCategoriesForProductAsync(int productId)
        {
            var productExists = await _productRepository
                .GetByIdReadOnlyAsync(productId);

            if (productExists is null)
                throw new KeyNotFoundException(
                    $"Product {productId} not found.");

            return await _categoryRepository
                .GetCategoriesForProductAsync(productId);
        }
    }
}
