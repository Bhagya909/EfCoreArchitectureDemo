namespace Application.Models
{
    public class OrderQueryParameters
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Status { get; set; }
        public int? CustomerId { get; set; }
    }
}