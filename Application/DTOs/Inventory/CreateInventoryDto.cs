using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Inventory
{
    public class CreateInventoryDto
    {
        public int ProductId { get; set; }

        public int Quantity { get; set; }
    }
}
