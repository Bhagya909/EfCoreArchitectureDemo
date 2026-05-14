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


    private Order()
    {
    }

    public Order(
        int customerId,
        decimal totalAmount)
    {
        CustomerId = customerId;
        TotalAmount = totalAmount;
        Status = OrderStatus.PendingPayment;
    }

    public void AddOrderItem(OrderItem item)
    {
        OrderItems.Add(item);
    }

 
    public void MarkAsCompleted()
    {
        Status = OrderStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }


    public void MarkAsPaid()
    {
        Status = OrderStatus.Paid;

        UpdatedAt = DateTime.UtcNow;
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