using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Inventory
{
    public class InventoryResponseDto
    {
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
