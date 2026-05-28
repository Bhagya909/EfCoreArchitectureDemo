namespace Domain.Entities.Orders;

public class Customer : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;

    public ICollection<Order> Orders { get; set; }
        = new List<Order>();

    private Customer() { }

    public Customer(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Customer name cannot be empty.");

        if (name.Length > 200)
            throw new ArgumentException(
                "Customer name cannot exceed 200 characters.");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                "Email cannot be empty.");

        if (email.Length > 255)
            throw new ArgumentException(
                "Email cannot exceed 255 characters.");

        if (!email.Contains('@'))
            throw new ArgumentException(
                "Email is not valid.");

        Name = name;
        Email = email.Trim().ToLowerInvariant();
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Customer name cannot be empty.");

        if (name.Length > 200)
            throw new ArgumentException(
                "Customer name cannot exceed 200 characters.");

        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}