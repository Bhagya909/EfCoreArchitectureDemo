using Application.DTOs.Orders;
using Domain.Entities.Orders;

namespace Application.Mappings;

public static class CustomerMappings
{
    public static CustomerResponseDto ToResponseDto(
        this Customer customer)
    {
        return new CustomerResponseDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            CreatedAt = customer.CreatedAt
        };
    }
}
