using Domain.Entities.Inventory;
using Domain.Entities.Orders;

namespace Domain.Entities.Catalog;

public class Product : BaseEntity
{
    public string Name { get; set; } = null!;

    public string SKU { get; set; } = null!;

    public decimal BasePrice { get; set; }

    public byte[] RowVersion { get; set; } = default!;

    public Inventory.Inventory? Inventory { get; set; }

    public ICollection<ProductCategory> ProductCategories { get; set; }
        = new List<ProductCategory>();

    public ICollection<OrderItem> OrderItems { get; set; }
        = new List<OrderItem>();

    public ICollection<InventoryTransaction> InventoryTransactions { get; set; }
        = new List<InventoryTransaction>();
}