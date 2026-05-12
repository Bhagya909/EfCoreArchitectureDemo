using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Orders
{
    public class OrderResponseDto
    {
        public int OrderId { get; set; }

        public string Status { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
