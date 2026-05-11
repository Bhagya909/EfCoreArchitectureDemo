using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Orders
{
    public class CreateOrderItemDto
    {
        public int ProductId { get; set; }

        public int Quantity { get; set; }
    }
}
