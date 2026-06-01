using Application.Common;
using Application.DTOs.Categories;
using Application.DTOs.Inventory;
using Application.DTOs.Logging;
using Application.DTOs.Orders;
using Application.DTOs.Payments;
using Application.DTOs.Products;
using Application.Models;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.Swagger;

public class RetailSwaggerSchemaFilter : ISchemaFilter
{
    private static readonly Dictionary<Type, string> TypeDescriptions = new()
    {
        [typeof(CreateProductDto)] = "Request body for creating a sellable catalog product.",
        [typeof(UpdateProductDto)] = "Request body for updating a catalog product with optimistic concurrency.",
        [typeof(ProductResponseDto)] = "Catalog product returned by read and write endpoints.",
        [typeof(BulkPriceUpdateDto)] = "Request body for changing prices across many products.",
        [typeof(BulkArchiveProductsDto)] = "Request body for archiving matching products in bulk.",
        [typeof(BulkRestoreProductsDto)] = "Request body for restoring archived products in bulk.",
        [typeof(CreateCategoryDto)] = "Request body for creating a product category.",
        [typeof(UpdateCategoryDto)] = "Request body for renaming a product category.",
        [typeof(AssignCategoryDto)] = "Request body for assigning a category to a product.",
        [typeof(CategoryResponseDto)] = "Category summary returned by catalog endpoints.",
        [typeof(CategoryDetailResponseDto)] = "Category details including active product count.",
        [typeof(ProductCategoryResponseDto)] = "Product/category assignment summary.",
        [typeof(CreateCustomerDto)] = "Request body for creating a customer account.",
        [typeof(UpdateCustomerDto)] = "Request body for updating customer profile data.",
        [typeof(CustomerResponseDto)] = "Customer summary returned by customer endpoints.",
        [typeof(CustomerDetailResponseDto)] = "Customer details including aggregate order count.",
        [typeof(CreateOrderDto)] = "Request body for creating an order with one or more line items.",
        [typeof(CreateOrderItemDto)] = "Single product line requested during order creation.",
        [typeof(OrderResponseDto)] = "Order read model including status, totals, and line items.",
        [typeof(OrderItemResponseDto)] = "Single line item returned with an order.",
        [typeof(CompletePaymentDto)] = "Request body for completing payment for a pending order.",
        [typeof(PaymentResponseDto)] = "Payment read model returned by payment endpoints.",
        [typeof(CreateInventoryDto)] = "Request body for adding stock to an existing product.",
        [typeof(InventoryResponseDto)] = "Current inventory position for a product.",
        [typeof(InventoryTransactionResponseDto)] = "Inventory movement record.",
        [typeof(InventorySummaryDto)] = "Aggregate inventory metrics for dashboards.",
        [typeof(StockValidationResultDto)] = "Result of checking whether requested stock is available.",
        [typeof(OperationResultDto)] = "Standard command result with a message and affected row count.",
        [typeof(ProductQueryParameters)] = "Query string filters used when listing products.",
        [typeof(OrderQueryParameters)] = "Query string filters used when listing orders.",
        [typeof(ChangeLogQueryParameters)] = "Query string filters used when listing change logs.",
        [typeof(ChangeLogResponseDto)] = "Change-log summary entry.",
        [typeof(ChangeLogDetailResponseDto)] = "Detailed change-log entry including audit and AI summary metadata.",
        [typeof(ChangeLogSummaryDto)] = "Aggregate counts for change-log and AI summary status.",
        [typeof(BulkDeleteOldLogsDto)] = "Request body for deleting old logs by age.",
        [typeof(BulkDeleteLogsByActionTypeDto)] = "Request body for deleting old logs by action type."
    };

    private static readonly Dictionary<string, string> PropertyDescriptions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = "Resource identifier.",
            ["productId"] = "Product identifier.",
            ["productName"] = "Display name of the product.",
            ["name"] = "Human-readable display name.",
            ["sku"] = "Unique stock keeping unit.",
            ["basePrice"] = "Base selling price.",
            ["rowVersion"] = "Base64-encoded concurrency token required for safe updates.",
            ["categoryId"] = "Category identifier.",
            ["categoryName"] = "Display name of the category.",
            ["productCount"] = "Number of active products assigned to the category.",
            ["customerId"] = "Customer identifier.",
            ["email"] = "Customer email address.",
            ["orderId"] = "Order identifier.",
            ["items"] = "Order line items.",
            ["quantity"] = "Quantity of product units.",
            ["status"] = "Current lifecycle status.",
            ["totalAmount"] = "Total monetary amount.",
            ["unitPrice"] = "Price captured for one unit.",
            ["lineTotal"] = "Line quantity multiplied by unit price.",
            ["paymentId"] = "Payment identifier.",
            ["amount"] = "Payment amount.",
            ["paidAt"] = "UTC timestamp when payment completed.",
            ["referenceNumber"] = "Generated payment reference number.",
            ["quantityChange"] = "Signed stock movement quantity.",
            ["transactionType"] = "Inventory transaction direction, such as IN or OUT.",
            ["reason"] = "Business reason recorded for the inventory movement.",
            ["lastUpdated"] = "UTC timestamp when the inventory position last changed.",
            ["createdAt"] = "UTC creation timestamp.",
            ["updatedAt"] = "UTC last update timestamp, when available.",
            ["isAvailable"] = "Indicates whether the requested quantity is available.",
            ["message"] = "Human-readable command result message.",
            ["affectedRows"] = "Number of records affected by the command.",
            ["page"] = "One-based page number.",
            ["pageNumber"] = "One-based page number.",
            ["pageSize"] = "Number of records returned per page.",
            ["searchTerm"] = "Optional search text.",
            ["sortBy"] = "Optional sort field.",
            ["totalCount"] = "Total number of records matching the query.",
            ["totalProductsTracked"] = "Number of products with inventory records.",
            ["totalUnitsInStock"] = "Total quantity across all tracked products.",
            ["outOfStockCount"] = "Number of tracked products with zero units.",
            ["lowStockCount"] = "Number of tracked products at or below the threshold.",
            ["lowStockThreshold"] = "Threshold used to classify low-stock products.",
            ["actionType"] = "Business or audit action name.",
            ["entityName"] = "Entity affected by the log entry.",
            ["referenceId"] = "Identifier of the affected entity, when available.",
            ["description"] = "Human-readable log description.",
            ["rawData"] = "Raw key-value details captured with the log.",
            ["logSource"] = "Source system that created the log.",
            ["correlationId"] = "Identifier used to correlate related operations.",
            ["category"] = "Log category.",
            ["severity"] = "Log severity.",
            ["changedFields"] = "Fields changed by an audited update.",
            ["oldValues"] = "Previous values for audited fields.",
            ["newValues"] = "New values for audited fields.",
            ["aiSummaryStatus"] = "AI summary processing status.",
            ["aiSummary"] = "Generated AI summary, when available.",
            ["aiSummaryError"] = "AI summary failure details, when available.",
            ["aiSummaryGeneratedAt"] = "UTC timestamp when the AI summary was generated.",
            ["olderThanDays"] = "Deletes records older than this many days.",
            ["fromDate"] = "Optional UTC start date filter.",
            ["toDate"] = "Optional UTC end date filter."
        };

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (TypeDescriptions.TryGetValue(context.Type, out var description) &&
            string.IsNullOrWhiteSpace(schema.Description))
        {
            schema.Description = description;
        }

        if (schema.Properties is null)
            return;

        foreach (var property in schema.Properties)
        {
            if (PropertyDescriptions.TryGetValue(property.Key, out var propertyDescription) &&
                string.IsNullOrWhiteSpace(property.Value.Description))
            {
                property.Value.Description = propertyDescription;
            }
        }
    }
}
