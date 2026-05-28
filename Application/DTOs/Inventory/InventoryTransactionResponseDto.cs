namespace Application.DTOs.Inventory;

public class InventoryTransactionResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string SKU { get; set; } = null!;
    public int QuantityChange { get; set; }
    public string TransactionType { get; set; } = null!;
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}