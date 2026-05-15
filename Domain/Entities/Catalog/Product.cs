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


    private Product()
    {
    }

    public Product(
        string name,
        string sku,
        decimal basePrice)
    {
        Name = name;
        SKU = sku;
        BasePrice = basePrice;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Price cannot be negative.");

        BasePrice = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }
}

