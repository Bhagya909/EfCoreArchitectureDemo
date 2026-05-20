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

        builder.Property(cl => cl.Description)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(cl => cl.CreatedAt);

        builder.HasIndex(cl => cl.ActionType);

        builder.HasIndex(cl => cl.LogSource);

        builder.HasIndex(cl => cl.CorrelationId);

        builder.HasIndex(cl => cl.AiSummaryStatus);

        builder.HasQueryFilter(cl => !cl.IsDeleted);

        builder.Property(cl => cl.Category)
        .HasMaxLength(50);

        builder.Property(cl => cl.Severity)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(cl => cl.LogSource)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(cl => cl.AiSummaryStatus)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(cl => cl.CorrelationId)
            .IsRequired();

        builder.Property(cl => cl.ChangedFields)
            .HasColumnType("nvarchar(max)");

        builder.Property(cl => cl.OldValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(cl => cl.NewValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(cl => cl.AiSummary)
            .HasColumnType("nvarchar(max)");

        builder.Property(cl => cl.AiSummaryError)
            .HasColumnType("nvarchar(max)");

        builder.Property(cl => cl.AiSummaryGeneratedAt);
    }
}