using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Products
{
    public class ProductResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        public decimal BasePrice { get; set; }
    }
}
