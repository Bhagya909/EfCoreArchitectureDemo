using Application.DTOs.Products;
using Domain.Entities.Catalog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Mappings
{
    public static class ProductMappings
    {
        public static ProductResponseDto ToResponseDto(
            this Product product)
        {
            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                BasePrice = product.BasePrice,
                RowVersion = product.RowVersion
            };
        }
    }
}
