namespace Application.DTOs.Inventory;

/// <summary>
/// Current stock position for a product.
/// </summary>
public class InventoryResponseDto
{
    /// <summary>
    /// Product identifier.
    /// </summary>
    public int ProductId { get; set; }
    /// <summary>
    /// Product display name.
    /// </summary>
    public string ProductName { get; set; } = null!;
    /// <summary>
    /// Product stock keeping unit.
    /// </summary>
    public string SKU { get; set; } = null!;
    /// <summary>
    /// Units currently available.
    /// </summary>
    public int Quantity { get; set; }
    /// <summary>
    /// UTC time when inventory was last changed.
    /// </summary>
    public DateTime LastUpdated { get; set; }
}
