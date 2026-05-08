using Domain.Enums;

namespace Domain.Entities.Orders;

public class Payment : BaseEntity
{
    public int OrderId { get; set; }

    public PaymentStatus Status { get; set; }

    public decimal Amount { get; set; }

    public DateTime? PaidAt { get; set; }

    public byte[] RowVersion { get; set; } = default!;

    public Order Order { get; set; } = null!;
}