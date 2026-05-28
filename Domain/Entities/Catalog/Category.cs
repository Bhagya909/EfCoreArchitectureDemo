namespace Domain.Entities.Catalog;

public class Category : BaseEntity
{
    public string Name { get; private set; } = null!;

    public ICollection<ProductCategory> ProductCategories { get; private set; }
        = new List<ProductCategory>();

    private Category() { }

    public Category(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Category name cannot be empty.");

        Name = name.Trim();
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Category name cannot be empty.");

        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}