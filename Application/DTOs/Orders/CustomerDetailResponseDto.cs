namespace Application.DTOs.Orders;

public class CustomerDetailResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int TotalOrders { get; set; }
}
