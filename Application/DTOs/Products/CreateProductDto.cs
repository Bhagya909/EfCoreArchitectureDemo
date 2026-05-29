using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Products
{
    /// <summary>
    /// Request used to create a new sellable catalog product.
    /// </summary>
    public class CreateProductDto
    {
        /// <summary>
        /// Display name shown to operators and customers.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Unique stock keeping unit used to identify the product.
        /// </summary>
        public string SKU { get; set; } = string.Empty;

        /// <summary>
        /// Base selling price. Must be greater than zero.
        /// </summary>
        public decimal BasePrice { get; set; }
    }
}
