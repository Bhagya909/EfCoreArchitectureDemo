using Domain.Entities.Inventory;
using InventoryNS = Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Inventory;

public class InventoryTransactionConfiguration
    : IEntityTypeConfiguration<InventoryNS.InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryNS.InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransactions");

        builder.HasKey(it => it.Id);

        builder.Property(it => it.QuantityChange)
            .IsRequired();

        builder.Property(it => it.TransactionType)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(it => it.Reason)
            .HasMaxLength(500);

        builder.HasIndex(it => it.CreatedAt);

        builder.HasQueryFilter(it => !it.IsDeleted);

        builder.HasOne(it => it.Product)
            .WithMany(p => p.InventoryTransactions)
            .HasForeignKey(it => it.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}