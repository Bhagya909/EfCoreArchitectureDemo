using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Inventory;

public class InventorySummaryDto
{
    public int TotalProductsTracked { get; set; }
    public int TotalUnitsInStock { get; set; }
    public int OutOfStockCount { get; set; }
    public int LowStockCount { get; set; }
    public int LowStockThreshold { get; set; }
}
