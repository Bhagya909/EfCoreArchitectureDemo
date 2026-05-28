using Domain.Enums;

namespace Domain.Entities.Orders;

public class Order : BaseEntity
{
    public int CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public byte[] RowVersion { get; private set; } = default!;
    public Customer Customer { get; private set; } = null!;
    public Payment? Payment { get; private set; }
    public ICollection<OrderItem> OrderItems { get; private set; }
        = new List<OrderItem>();

    private Order() { }

    public Order(
        int customerId,
        decimal totalAmount)
    {
        if (customerId <= 0)
            throw new ArgumentException(
                "CustomerId must be greater than zero.");

        if (totalAmount <= 0)
            throw new ArgumentException(
                "TotalAmount must be greater than zero.");

        CustomerId = customerId;
        TotalAmount = totalAmount;
        Status = OrderStatus.PendingPayment;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddOrderItem(OrderItem item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        OrderItems.Add(item);
    }

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.PendingPayment)
            throw new InvalidOperationException(
                "Only orders pending payment can be marked as paid.");

        Status = OrderStatus.Paid;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted()
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException(
                "Only paid orders can be completed.");

        Status = OrderStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status != OrderStatus.PendingPayment)
            throw new InvalidOperationException(
                "Only orders pending payment can be cancelled.");

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}