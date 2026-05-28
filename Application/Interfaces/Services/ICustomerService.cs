using Application.DTOs.Orders;

namespace Application.Interfaces.Services;

public interface ICustomerService
{
    Task<CustomerResponseDto> CreateCustomerAsync(
        CreateCustomerDto dto);

    Task<CustomerDetailResponseDto?> GetCustomerByIdAsync(int id);

    Task<List<CustomerResponseDto>> GetAllCustomersAsync();

    Task<CustomerResponseDto?> UpdateCustomerAsync(
        int id,
        UpdateCustomerDto dto);

    Task<bool> SoftDeleteCustomerAsync(int id);

    Task<List<OrderResponseDto>> GetCustomerOrdersAsync(
        int id);
}
