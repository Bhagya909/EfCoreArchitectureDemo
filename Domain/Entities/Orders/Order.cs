using Domain.Enums;

namespace Domain.Entities.Orders;

public class Order : BaseEntity
{
    public int CustomerId { get; set; }

    public OrderStatus Status { get; set; }

    public decimal TotalAmount { get; set; }

    public byte[] RowVersion { get; set; } = default!;

    public Customer Customer { get; set; } = null!;

    public Payment? Payment { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; }
        = new List<OrderItem>();
}