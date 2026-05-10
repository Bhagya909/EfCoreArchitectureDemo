using Domain.Entities.Inventory;
using InventoryNS = Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Inventory;

public class InventoryConfiguration : IEntityTypeConfiguration<InventoryNS.Inventory>
{
    public void Configure(EntityTypeBuilder<InventoryNS.Inventory> builder)
    {
        builder.ToTable("Inventories");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.Property(i => i.LastUpdated)
            .IsRequired();

        builder.Property(i => i.RowVersion)
            .IsRowVersion();

        builder.HasIndex(i => i.ProductId)
            .IsUnique();

        builder.HasQueryFilter(i => !i.IsDeleted);

        builder.HasOne(i => i.Product)
            .WithOne(p => p.Inventory)
            .HasForeignKey<InventoryNS.Inventory>(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}