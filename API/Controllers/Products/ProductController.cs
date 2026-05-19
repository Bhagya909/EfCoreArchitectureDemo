using Application.Common;
using Application.DTOs.Products;
using Application.Interfaces.Services;
using Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Products
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(
            IProductService productService)
        {
            _productService = productService;
        }


        [HttpGet]
        public async Task<ActionResult<
            PagedResult<ProductResponseDto>>>
            GetAll(
                [FromQuery]
            ProductQueryParameters queryParameters)
        {
            var products =
                await _productService
                    .GetAllProductsAsync(
                        queryParameters);

            return Ok(products);
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<
            ProductResponseDto>>
            GetById(int id)
        {
            var product =
                await _productService
                    .GetProductByIdAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            return Ok(product);
        }


        [HttpPost]
        public async Task<ActionResult<
            ProductResponseDto>>
            Create(
                CreateProductDto dto)
        {
            var createdProduct =
                await _productService
                    .CreateProductAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdProduct.Id },
                createdProduct);
        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult>
            Delete(int id)
        {
            var deleted =
                await _productService
                    .SoftDeleteProductAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
