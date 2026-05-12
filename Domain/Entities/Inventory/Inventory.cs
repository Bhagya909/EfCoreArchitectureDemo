using Domain.Entities.Catalog;
using Domain.Enums;
using System;

namespace Domain.Entities.Inventory;

public class Inventory : BaseEntity
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public DateTime LastUpdated { get; set; }

    public byte[] RowVersion { get; private set; } = default!;

    public Product Product { get; set; } = null!;

    // Navigation Property
    public ICollection<InventoryTransaction> InventoryTransactions
    { get; private set; }
        = new List<InventoryTransaction>();


    // EF Core constructor
    private Inventory()
    {
    }

    public Inventory(
        int productId,
        int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
        LastUpdated = DateTime.UtcNow;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        Quantity += quantity;
        LastUpdated = DateTime.UtcNow;
    }

    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        if (Quantity < quantity)
            throw new InvalidOperationException("Insufficient stock.");

        Quantity -= quantity;
        LastUpdated = DateTime.UtcNow;
    }
}