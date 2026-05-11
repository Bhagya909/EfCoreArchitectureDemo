using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Orders
{
    public class CreateOrderDto
    {
        public int CustomerId { get; set; }

        public List<CreateOrderItemDto> Items { get; set; } = [];
    }
}
