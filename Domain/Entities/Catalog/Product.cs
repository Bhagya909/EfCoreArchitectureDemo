using Domain.Entities.Inventory;
using Domain.Entities.Orders;

namespace Domain.Entities.Catalog;

public class Product : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string SKU { get; private set; } = null!;
    public decimal BasePrice { get; private set; }
    public byte[] RowVersion { get; private set; } = default!;

    public Inventory.Inventory? Inventory { get; private set; }

    public ICollection<ProductCategory> ProductCategories { get; private set; }
        = new List<ProductCategory>();

    public ICollection<OrderItem> OrderItems { get; private set; }
        = new List<OrderItem>();

    public ICollection<InventoryTransaction> InventoryTransactions { get; private set; }
        = new List<InventoryTransaction>();


    private Product()
    {
    }

    public Product(
        string name,
        string sku,
        decimal basePrice)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Product name cannot be empty.");

        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException(
                "SKU cannot be empty.");

        if (basePrice <= 0)
            throw new ArgumentException(
                "Price must be greater than zero.");

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
        if (newPrice <= 0)
            throw new ArgumentException("Price must be greater than zero.");

        BasePrice = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(
    string name,
    string sku,
    decimal basePrice)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Product name cannot be empty.");

        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException(
                "SKU cannot be empty.");

        if (basePrice <= 0)
            throw new ArgumentException(
                "Price must be greater than zero.");

        Name = name;
        SKU = sku;
        BasePrice = basePrice;
        UpdatedAt = DateTime.UtcNow;
    }
    public void SetRowVersion(byte[] rowVersion)
    {
        if (rowVersion is null || rowVersion.Length == 0)
            throw new ArgumentException(
                "RowVersion cannot be empty.");

        RowVersion = rowVersion;
    }
}

