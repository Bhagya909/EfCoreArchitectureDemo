namespace Application.DTOs.Products
{
    /// <summary>
    /// Product representation returned by catalog endpoints.
    /// </summary>
    public class ProductResponseDto
    {
        /// <summary>
        /// Product identifier.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Product display name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Unique stock keeping unit.
        /// </summary>
        public string SKU { get; set; } = string.Empty;
        /// <summary>
        /// Current base selling price.
        /// </summary>
        public decimal BasePrice { get; set; }
        /// <summary>
        /// Concurrency token clients must send back when updating the product.
        /// </summary>
        public byte[] RowVersion { get; set; } = default!;
    }
}
