using Domain.Entities.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Logging;

public class ChangeLogConfiguration : IEntityTypeConfiguration<ChangeLog>
{
    public void Configure(EntityTypeBuilder<ChangeLog> builder)
    {
        builder.ToTable("ChangeLogs");

        builder.HasKey(cl => cl.Id);

        builder.Property(cl => cl.ActionType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(cl => cl.EntityName)
            .HasMaxLength(100);

        builder.Property(cl => cl.RawData)
            .HasColumnType("nvarchar(max)");

        builder.Property(cl => cl.Summary)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(cl => cl.CreatedAt);

        builder.HasIndex(cl => cl.ActionType);

        builder.HasQueryFilter(cl => !cl.IsDeleted);
    }
}