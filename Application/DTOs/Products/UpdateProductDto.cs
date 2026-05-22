namespace Application.DTOs.Products;

public class UpdateProductDto
{
    public string Name { get; set; } = null!;
    public string SKU { get; set; } = null!;
    public decimal BasePrice { get; set; }
}