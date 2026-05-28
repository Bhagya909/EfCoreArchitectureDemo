using Domain.Enums;

namespace Domain.Entities.Orders;

public class Payment : BaseEntity
{
    public int OrderId { get; private set; }
    public PaymentStatus Status { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public byte[] RowVersion { get; private set; } = default!;
    public string? ReferenceNumber { get; private set; }
    public Order Order { get; private set; } = null!;

    private Payment() { }

    public Payment(int orderId, decimal amount)
    {
        if (orderId <= 0)
            throw new ArgumentException(
                "OrderId must be greater than zero.");

        if (amount <= 0)
            throw new ArgumentException(
                "Amount must be greater than zero.");

        OrderId = orderId;
        Amount = amount;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException(
                "Only pending payments can be completed.");

        Status = PaymentStatus.Completed;
        PaidAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        if (Status == PaymentStatus.Completed)
            throw new InvalidOperationException(
                "Completed payments cannot be marked as failed.");

        Status = PaymentStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetReferenceNumber(string referenceNumber)
    {
        if (string.IsNullOrWhiteSpace(referenceNumber))
            throw new ArgumentException(
                "Reference number cannot be empty.");

        ReferenceNumber = referenceNumber;
    }
}