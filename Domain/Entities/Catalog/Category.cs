namespace Domain.Entities.Catalog;

public class Category : BaseEntity
{
    public string Name { get; set; } = null!;

    public ICollection<ProductCategory> ProductCategories { get; set; }
        = new List<ProductCategory>();
}