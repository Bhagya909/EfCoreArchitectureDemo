using Application.DTOs.Inventory;
using Domain.Entities.Inventory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Mappings
{
    public static class InventoryMappings
    {
        public static InventoryResponseDto ToResponseDto(
            this Inventory inventory)
        {
            return new InventoryResponseDto
            {
                ProductId = inventory.ProductId,
                Quantity = inventory.Quantity,
                LastUpdated = inventory.LastUpdated
            };
        }
    }
}
