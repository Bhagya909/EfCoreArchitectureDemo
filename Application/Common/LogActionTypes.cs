namespace Application.Common;

public static class LogActionTypes
{
    public const string ProductCreated = "PRODUCT_CREATED";
    public const string ProductUpdated = "PRODUCT_UPDATED";
    public const string ProductDeleted = "PRODUCT_DELETED";
    public const string BulkPriceUpdate = "BULK_PRICE_UPDATE";
    public const string BulkProductArchive = "BULK_PRODUCT_ARCHIVE";
    public const string BulkRestore = "BULK_RESTORE";

    public const string CategoryCreated = "CATEGORY_CREATED";
    public const string CategoryUpdated = "CATEGORY_UPDATED";
    public const string CategoryDeleted = "CATEGORY_DELETED";
    public const string CategoryProductsArchived = "CATEGORY_PRODUCTS_ARCHIVED";
    public const string CategoryAssigned = "CATEGORY_ASSIGNED";
    public const string CategoryRemoved = "CATEGORY_REMOVED";

    public const string CustomerCreated = "CUSTOMER_CREATED";
    public const string CustomerUpdated = "CUSTOMER_UPDATED";
    public const string CustomerDeleted = "CUSTOMER_DELETED";

    public const string InventoryAdded = "INVENTORY_ADD";
    public const string InventoryDeducted = "INVENTORY_DEDUCTED";

    public const string OrderCreated = "ORDER_CREATED";
    public const string OrderCancelled = "ORDER_CANCELLED";
    public const string OrderCompleted = "ORDER_COMPLETED";

    public const string PaymentCompleted = "PAYMENT_COMPLETED";
    public const string PaymentFailed = "PAYMENT_FAILED";

    public const string UpgradeExecuted = "UPGRADE_EXECUTED";
}
