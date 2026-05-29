using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Inventory
{
    /// <summary>
    /// Request used to add stock for a product.
    /// </summary>
    public class CreateInventoryDto
    {
        /// <summary>
        /// Product receiving additional stock.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Quantity to add. Must be greater than zero.
        /// </summary>
        public int Quantity { get; set; }
    }
}
