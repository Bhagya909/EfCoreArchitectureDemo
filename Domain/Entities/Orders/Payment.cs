using Domain.Enums;

namespace Domain.Entities.Orders;

public class Payment : BaseEntity
{
    public int OrderId { get; set; }

    public PaymentStatus Status { get; set; }

    public decimal Amount { get; set; }

    public DateTime? PaidAt { get; set; }

    public byte[] RowVersion { get; set; } = default!;

    public string? ReferenceNumber { get; private set; }

    public Order Order { get; set; } = null!;


    private Payment()
    {
    }

    public Payment(
        int orderId,
        decimal amount)
    {
        OrderId = orderId;

        Amount = amount;

        Status = PaymentStatus.Pending;

        CreatedAt = DateTime.UtcNow;
    }


    public void MarkAsCompleted()
    {
        Status = PaymentStatus.Completed;

        PaidAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        if (Status == PaymentStatus.Completed)
            throw new InvalidOperationException("Completed payment cannot fail.");

        Status = PaymentStatus.Failed;
    }

    public void SetReferenceNumber(string referenceNumber)
    {
        if (string.IsNullOrWhiteSpace(referenceNumber))
        {
            throw new ArgumentException(
                "Reference number cannot be empty.");
        }

        ReferenceNumber = referenceNumber;
    }

}