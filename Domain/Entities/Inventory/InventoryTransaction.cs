using Domain.Entities.Catalog;
using Domain.Enums;

namespace Domain.Entities.Inventory;

public class InventoryTransaction : BaseEntity
{
    public int ProductId { get; private set; }

    public int QuantityChange { get; private set; }

    public InventoryTransactionType TransactionType { get; private set; }

    public string? Reason { get; private set; }

    public Product Product { get; private set; } = null!;

    private InventoryTransaction()
    {
    }

    public InventoryTransaction(
        int productId,
        int quantityChange,
        InventoryTransactionType type,
        string? reason)
    {
        if (productId <= 0)
            throw new ArgumentException(
                "ProductId must be greater than zero.");

        if (quantityChange == 0)
            throw new ArgumentException(
                "QuantityChange cannot be zero.");

        if (!Enum.IsDefined(type))
            throw new ArgumentException(
                "Transaction type is not valid.");

        if (type == InventoryTransactionType.IN
            && quantityChange < 0)
            throw new ArgumentException(
                "IN transaction must have positive quantity.");

        if (type == InventoryTransactionType.OUT
            && quantityChange > 0)
            throw new ArgumentException(
                "OUT transaction must have negative quantity.");

        ProductId = productId;
        QuantityChange = quantityChange;
        TransactionType = type;
        Reason = reason;
    }
}
