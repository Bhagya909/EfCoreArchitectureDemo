using Application.DTOs.Orders;
using Domain.Entities.Orders;

namespace Application.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id);
    Task<bool> ExistsByEmailAsync(string email);
    Task<List<CustomerResponseDto>> GetAllAsync();
    Task AddAsync(Customer customer);
    Task<CustomerDetailResponseDto?> GetByIdWithOrderCountAsync(int id);
    Task<bool> ExistsByIdAsync(int id);
}
