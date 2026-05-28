using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RetailProject.Tests.Fixtures;

public class TestRetailDbContext : RetailDbContext
{
    public TestRetailDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                // Strip nvarchar(max)
                var columnType = property.GetColumnType();
                if (!string.IsNullOrWhiteSpace(columnType) &&
                    columnType.Contains("max",
                        StringComparison.OrdinalIgnoreCase))
                {
                    property.SetColumnType(null);
                }

                // Disable server-generated RowVersion for SQLite
                if (property.ClrType == typeof(byte[]) &&
                    property.ValueGenerated ==
                        ValueGenerated.OnAddOrUpdate)
                {
                    property.SetColumnType("BLOB");
                    property.ValueGenerated =
                        ValueGenerated.Never;
                    property.SetDefaultValueSql(null);
                }
            }

            // Strip filtered index syntax
            foreach (var index in entityType.GetIndexes())
            {
                index.SetFilter(null);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        // SQLite cannot generate RowVersion — set a non-null
        // byte[] on any Added entity that has one as null
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added)
                continue;

            var rowVersionProp = entry.Properties
                .FirstOrDefault(p =>
                    p.Metadata.ClrType == typeof(byte[]) &&
                    p.Metadata.ValueGenerated ==
                        ValueGenerated.Never &&
                    p.CurrentValue is null);

            if (rowVersionProp is not null)
                rowVersionProp.CurrentValue =
                    Array.Empty<byte>();
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}