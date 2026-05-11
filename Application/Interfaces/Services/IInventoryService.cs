using Application.DTOs.Inventory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IInventoryService
    {
        Task<InventoryResponseDto> AddInventoryAsync(
        CreateInventoryDto dto);

        Task<bool> ValidateStockAsync(
            int productId,
            int quantity);

        Task DeductStockAsync(
            int productId,
            int quantity);
    }
}
