namespace Application.Common;

/// <summary>
/// Standard response used by command endpoints that return an operational message and affected row count.
/// </summary>
public class OperationResultDto
{
    /// <summary>
    /// Human-readable result message suitable for Swagger demos and API clients.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Number of rows or records affected by the operation.
    /// </summary>
    public int AffectedRows { get; set; }
}

/// <summary>
/// Response returned by inventory stock validation.
/// </summary>
public class StockValidationResultDto
{
    /// <summary>
    /// Product identifier whose inventory was checked.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Quantity requested by the caller.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Indicates whether the requested quantity is currently available.
    /// </summary>
    public bool IsAvailable { get; set; }
}
