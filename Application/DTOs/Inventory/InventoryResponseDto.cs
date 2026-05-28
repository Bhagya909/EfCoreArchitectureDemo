namespace Application.DTOs.Inventory;

public class InventoryResponseDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string SKU { get; set; } = null!;
    public int Quantity { get; set; }
    public DateTime LastUpdated { get; set; }
}