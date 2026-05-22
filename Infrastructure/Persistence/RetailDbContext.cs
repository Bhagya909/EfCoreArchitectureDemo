using Domain.Entities;
using Domain.Entities.Catalog;
using Domain.Entities.Inventory;
using Domain.Entities.Logging;
using Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Persistence;

public class RetailDbContext : DbContext
{
    public RetailDbContext(DbContextOptions<RetailDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();

    public DbSet<Inventory> Inventories => Set<Inventory>();

    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<ChangeLog> ChangeLogs => Set<ChangeLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(RetailDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker
            .Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(
            cancellationToken);
    }
}