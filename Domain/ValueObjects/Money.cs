namespace Domain.ValueObjects;

public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.");

        Amount = amount;
    }

    public bool Equals(Money? other)
    {
        if (other is null)
            return false;

        return Amount == other.Amount;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Money);
    }

    public override int GetHashCode()
    {
        return Amount.GetHashCode();
    }

    public static implicit operator decimal(Money money)
    {
        return money.Amount;
    }

    public static implicit operator Money(decimal amount)
    {
        return new Money(amount);
    }
}