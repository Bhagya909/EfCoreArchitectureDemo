using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Payments
{
    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }

        public string Status { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public DateTime? PaidAt { get; set; }
    }
}
