using Domain.Entities.Catalog;

namespace Domain.Entities.Orders;

public class OrderItem
{
    public int Id { get; private set; }
    public int OrderId { get; private set; }
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public Order Order { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    private OrderItem() { }

    public OrderItem(
        int productId,
        int quantity,
        decimal unitPrice)
    {
        if (productId <= 0)
            throw new ArgumentException(
                "ProductId must be greater than zero.");

        if (quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");

        if (unitPrice <= 0)
            throw new ArgumentException(
                "UnitPrice must be greater than zero.");

        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}