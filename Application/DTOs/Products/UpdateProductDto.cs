namespace Application.DTOs.Products;

/// <summary>
/// Request used to update product details with optimistic concurrency protection.
/// </summary>
public class UpdateProductDto
{
    /// <summary>
    /// Updated product display name.
    /// </summary>
    public string Name { get; set; } = null!;
    /// <summary>
    /// Updated unique stock keeping unit.
    /// </summary>
    public string SKU { get; set; } = null!;
    /// <summary>
    /// Updated base selling price. Must be greater than zero.
    /// </summary>
    public decimal BasePrice { get; set; }
    /// <summary>
    /// RowVersion returned by the last read. The API uses it to detect conflicting updates.
    /// </summary>
    public byte[] RowVersion { get; set; } = null!;
}
