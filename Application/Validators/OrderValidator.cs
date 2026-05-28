using Application.DTOs.Orders;

namespace Application.Validators;

public static class OrderValidator
{
    public static void ValidateCreateOrder(CreateOrderDto dto)
    {
        if (dto.CustomerId <= 0)
            throw new ArgumentException("Invalid customer id.");

        if (dto.Items.Count == 0)
            throw new ArgumentException(
                "Order must contain at least one item.");

        foreach (var item in dto.Items)
        {
            if (item.ProductId <= 0)
                throw new ArgumentException("Invalid product id.");

            if (item.Quantity <= 0)
                throw new ArgumentException(
                    "Quantity must be greater than zero.");
        }
    }
}