using Application.Common;
using Application.DTOs.Categories;
using Application.DTOs.Products;
using Application.Interfaces.Services;
using Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Products
{
    [ApiController]
    [Route("api/products")]
    [Produces("application/json")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ProductResponseDto>), 200)]
        public async Task<ActionResult<PagedResult<ProductResponseDto>>> GetAll(
            [FromQuery] ProductQueryParameters queryParameters)
        {
            var products = await _productService.GetAllProductsAsync(queryParameters);
            return Ok(products);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProductResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProductResponseDto>> GetById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product is null)
                return NotFound(new
                {
                    message = $"Product with id {id} was not found."
                });

            return Ok(product);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<ActionResult<ProductResponseDto>> Create(
            [FromBody] CreateProductDto dto)
        {
            var createdProduct = await _productService.CreateProductAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdProduct.Id },
                createdProduct);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ProductResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<ActionResult<ProductResponseDto>> Update(
            int id, [FromBody] UpdateProductDto dto)
        {
            var updated = await _productService.UpdateProductAsync(id, dto);

            if (updated is null)
                return NotFound(new
                {
                    message = $"Product with id {id} was not found."
                });

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _productService.SoftDeleteProductAsync(id);

            if (!deleted)
                return NotFound(new
                {
                    message = $"Product with id {id} was not found."
                });

            return NoContent();
        }

        [HttpPost("bulk-price-update")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> BulkPriceUpdate(
            [FromBody] BulkPriceUpdateDto dto)
        {
            var affectedRows = await _productService.BulkUpdatePricesAsync(dto);

            return Ok(new
            {
                message = $"{affectedRows} products price-updated successfully.",
                affectedRows
            });
        }

        [HttpPost("bulk-archive")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> BulkArchive(
            [FromBody] BulkArchiveProductsDto dto)
        {
            var affectedRows = await _productService.BulkArchiveProductsAsync(dto);

            return Ok(new
            {
                message = $"{affectedRows} products archived successfully.",
                affectedRows
            });
        }

        [HttpPost("bulk-restore")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> BulkRestore(
            [FromBody] BulkRestoreProductsDto dto)
        {
            var affectedRows = await _productService.BulkRestoreProductsAsync(dto);

            return Ok(new
            {
                message = $"{affectedRows} products restored successfully.",
                affectedRows
            });
        }

        [HttpPost("{id:int}/categories")]
        [ProducesResponseType(typeof(CategoryResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<ActionResult<CategoryResponseDto>> AssignCategory(
            int id, AssignCategoryDto dto)
        {
            var result = await _categoryService
                .AssignCategoryToProductAsync(id, dto);

            if (result is null)
                return NotFound(new
                {
                    message = $"Product with id {id} was not found."
                });

            return Ok(result);
        }

        [HttpDelete("{id:int}/categories/{categoryId:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> RemoveCategory(int id, int categoryId)
        {
            var removed = await _categoryService
                .RemoveCategoryFromProductAsync(id, categoryId);

            if (!removed)
                return NotFound(new
                {
                    message =
                        $"Assignment between Product {id} " +
                        $"and Category {categoryId} was not found."
                });

            return NoContent();
        }

        [HttpGet("{id:int}/categories")]
        [ProducesResponseType(typeof(List<ProductCategoryResponseDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<ProductCategoryResponseDto>>> GetCategories(
            int id)
        {
            var categories = await _categoryService
                .GetCategoriesForProductAsync(id);

            return Ok(categories);
        }
    }
}