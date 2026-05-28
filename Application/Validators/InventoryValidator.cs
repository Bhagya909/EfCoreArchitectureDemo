using Application.DTOs.Inventory;

namespace Application.Validators;

public static class InventoryValidator
{
    public static void ValidateInventory(CreateInventoryDto dto)
    {
        if (dto.ProductId <= 0)
            throw new ArgumentException("Invalid product id.");

        if (dto.Quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");
    }
}