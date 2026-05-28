namespace Application.DTOs.Products
{
    public class BulkPriceUpdateDto
    {
        public decimal PercentageChange { get; set; }
        public int? CategoryId { get; set; }
    }
}