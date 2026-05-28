using Domain.Entities.Catalog;
using System;

namespace Domain.Entities.Inventory;

public class Inventory : BaseEntity
{
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }
    public DateTime LastUpdated { get; private set; }
    public byte[] RowVersion { get; private set; } = default!;

    public Product Product { get; private set; } = null!;



    // EF Core constructor
    private Inventory()
    {
    }

    public Inventory(
        int productId,
        int quantity)
    {
        if (productId <= 0)
            throw new ArgumentException(
                "ProductId must be greater than zero.");

        if (quantity <= 0)
            throw new ArgumentException(
                "Initial quantity must be greater than zero.");

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