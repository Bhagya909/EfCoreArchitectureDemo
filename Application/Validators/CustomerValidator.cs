using Application.DTOs.Orders;

namespace Application.Validators;

public static class CustomerValidator
{
    public static void ValidateCreateCustomer(
        CreateCustomerDto dto)
    {
        var email = dto.Email?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException(
                "Customer name is required.");

        if (dto.Name.Length > 200)
            throw new ArgumentException(
                "Customer name cannot exceed 200 characters.");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                "Email is required.");

        if (email.Length > 255)
            throw new ArgumentException(
                "Email cannot exceed 255 characters.");

        if (!email.Contains('@'))
            throw new ArgumentException(
                "Email is not valid.");
    }

    public static void ValidateUpdateCustomer(
        UpdateCustomerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException(
                "Customer name is required.");

        if (dto.Name.Length > 200)
            throw new ArgumentException(
                "Customer name cannot exceed 200 characters.");
    }
}