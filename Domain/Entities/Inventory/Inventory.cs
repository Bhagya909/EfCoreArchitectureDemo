using Domain.Entities.Catalog;

namespace Domain.Entities.Inventory;

public class Inventory : BaseEntity
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public DateTime LastUpdated { get; set; }

    public byte[] RowVersion { get; set; } = default!;

    public Product Product { get; set; } = null!;
}