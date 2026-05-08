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


    public void MarkAsPaid()
    {
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Cancelled order cannot be paid.");

        Status = OrderStatus.Paid;
    }

    public void Complete()
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException("Only paid orders can be completed.");

        Status = OrderStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Completed order cannot be cancelled.");

        Status = OrderStatus.Cancelled;
    }
}