using Application.Common;
using Application.DTOs.Categories;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Products
{
    [ApiController]
    [Route("api/categories")]
    [Produces("application/json")]
    [Tags("Catalog - Categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Creates a product category used to organize and filter catalog items.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CategoryResponseDto>> Create(
            CreateCategoryDto dto)
        {
            var category = await _categoryService
                .CreateCategoryAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = category.Id },
                category);
        }

        /// <summary>
        /// Lists all active categories available for product assignment.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<CategoryResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CategoryResponseDto>>> GetAll()
        {
            var categories = await _categoryService
                .GetAllCategoriesAsync();

            return Ok(categories);
        }

        /// <summary>
        /// Gets category details, including the number of active products assigned to it.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CategoryDetailResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryDetailResponseDto>> GetById(
            int id)
        {
            var category = await _categoryService
                .GetCategoryByIdAsync(id);

            if (category is null)
                return NotFound(new
                {
                    message = $"Category with id {id} was not found."
                });

            return Ok(category);
        }

        /// <summary>
        /// Renames a category while preserving existing product assignments.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CategoryResponseDto>> Update(
            int id,
            UpdateCategoryDto dto)
        {
            var category = await _categoryService
                .UpdateCategoryAsync(id, dto);

            if (category is null)
                return NotFound(new
                {
                    message = $"Category with id {id} was not found."
                });

            return Ok(category);
        }

        /// <summary>
        /// Archives a category with soft delete so historical product assignments remain auditable.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _categoryService
                .SoftDeleteCategoryAsync(id);

            if (!deleted)
                return NotFound(new
                {
                    message = $"Category with id {id} was not found."
                });

            return NoContent();
        }

        /// <summary>
        /// Lists products assigned to a category using paginated read-model projection.
        /// </summary>
        [HttpGet("{id:int}/products")]
        [ProducesResponseType(typeof(PagedResult<ProductCategoryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PagedResult<ProductCategoryResponseDto>>>
            GetProductsByCategory(
                int id,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10)
        {
            var products = await _categoryService
                .GetProductsByCategoryAsync(id, page, pageSize);

            return Ok(products);
        }

        /// <summary>
        /// Archives every active product assigned to the category through a bulk database update.
        /// </summary>
        [HttpPost("{id:int}/archive-products")]
        [ProducesResponseType(typeof(OperationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ArchiveProducts(int id)
        {
            var affectedRows = await _categoryService
                .ArchiveProductsByCategoryAsync(id);

            return Ok(new
            {
                message =
                    $"{affectedRows} products archived " +
                    $"for category {id}.",
                affectedRows
            });
        }
    }
}
