using Domain.Entities.Catalog;

namespace Domain.Entities.Orders;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public Order Order { get; set; } = null!;

    public Product Product { get; set; } = null!;

    private OrderItem()
    {
    }

    public OrderItem(
        int productId,
        int quantity,
        decimal unitPrice)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}