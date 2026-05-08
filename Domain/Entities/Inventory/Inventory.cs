using System;
using Domain.Entities.Catalog;

namespace Domain.Entities.Inventory;

public class Inventory : BaseEntity
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public DateTime LastUpdated { get; set; }

    public byte[] RowVersion { get; set; } = default!;

    public Product Product { get; set; } = null!;

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