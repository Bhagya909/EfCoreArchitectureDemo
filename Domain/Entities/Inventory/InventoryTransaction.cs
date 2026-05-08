using Domain.Entities.Catalog;
using Domain.Enums;

namespace Domain.Entities.Inventory;

public class InventoryTransaction : BaseEntity
{
    public int ProductId { get; set; }

    public int QuantityChange { get; set; }

    public InventoryTransactionType TransactionType { get; set; }

    public string? Reason { get; set; }

    public Product Product { get; set; } = null!;
}