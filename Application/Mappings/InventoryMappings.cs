using Application.DTOs.Inventory;
using Domain.Entities.Catalog;
using Domain.Entities.Inventory;

namespace Application.Mappings;

public static class InventoryMappings
{
    public static InventoryResponseDto ToResponseDto(
        this Inventory inventory,
        Product product)
    {
        return new InventoryResponseDto
        {
            ProductId = inventory.ProductId,
            ProductName = product.Name,
            SKU = product.SKU,
            Quantity = inventory.Quantity,
            LastUpdated = inventory.LastUpdated
        };
    }
}
