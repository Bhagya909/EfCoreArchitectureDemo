using Domain.Entities.Logging;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace Infrastructure.Persistence.Interceptors;

public class AuditSaveChangesInterceptor
    : SaveChangesInterceptor
{
    public override async ValueTask<
    InterceptionResult<int>> SavingChangesAsync(
    DbContextEventData eventData,
    InterceptionResult<int> result,
    CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;

        if (context is null)
        {
            return await base.SavingChangesAsync(
                eventData,
                result,
                cancellationToken);
        }

        var entries = context.ChangeTracker
            .Entries()
            .Where(e =>
               e.Entity is not ChangeLog &&
                (
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified ||
                    e.State == EntityState.Deleted
                ))
             .ToList();

        foreach (var entry in entries)
        {
            var entityName =
                entry.Entity.GetType().Name;

            // Detect soft delete explicitly
            bool isSoftDelete =
                entry.State == EntityState.Modified
                && entry.Properties.Any(p =>
                    p.Metadata.Name == "IsDeleted"
                    && p.IsModified
                    && (bool?)p.CurrentValue == true);

            // Action type aware of soft delete
            var actionType = isSoftDelete
                ? "ENTITY_SOFT_DELETED"
                : entry.State switch
                {
                    EntityState.Added => "ENTITY_CREATED",
                    EntityState.Modified => "ENTITY_MODIFIED",
                    EntityState.Deleted => "ENTITY_DELETED",
                    _ => "ENTITY_CHANGED"
                };

            var excludedFields = new[]
            {
                "Id",
                "CreatedAt",
                "UpdatedAt",
                "RowVersion"
            };

            var changedProperties = entry.State switch
            {
                EntityState.Added =>
                    entry.Properties
                        .Where(p =>
                            !excludedFields.Contains(
                                p.Metadata.Name))
                        .ToList(),

                EntityState.Deleted =>
                    entry.Properties
                        .Where(p =>
                            !excludedFields.Contains(
                                p.Metadata.Name))
                        .ToList(),

                _ =>
                    entry.Properties
                        .Where(p =>
                            p.IsModified &&
                            !excludedFields.Contains(
                                p.Metadata.Name))
                        .ToList()
            };

            if (!changedProperties.Any())
            {
                continue;
            }

            var changedFields = new List<string>();
            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var property in changedProperties)
            {
                var propertyName = property.Metadata.Name;
                changedFields.Add(propertyName);
                oldValues[propertyName] = property.OriginalValue;
                newValues[propertyName] = property.CurrentValue;
            }

            var changedFieldsJson =
                JsonSerializer.Serialize(changedFields);

            var oldValuesJson =
                JsonSerializer.Serialize(oldValues);

            var newValuesJson =
                JsonSerializer.Serialize(newValues);

            // FIX 3 — action-specific description
            var description = isSoftDelete
                ? $"{entityName} was soft deleted."
                : actionType switch
                {
                    "ENTITY_CREATED" =>
                        $"{entityName} was created. " +
                        $"Fields: {string.Join(", ", changedFields)}.",
                    "ENTITY_DELETED" =>
                        $"{entityName} was deleted. " +
                        $"Fields: {string.Join(", ", changedFields)}.",
                    _ =>
                        $"{entityName} was modified. " +
                        $"Changed fields: {string.Join(", ", changedFields)}."
                };

            var auditLog = new ChangeLog(
                actionType: actionType,
                entityName: entityName,
                referenceId: null,
                rawData: $"{entityName} automatically audited.",
                description: description,
                logSource: LogSource.Audit,
                correlationId: Guid.NewGuid());

            // FIX 2 — set CreatedAt directly since interceptor
            // runs before SaveChangesAsync timestamp override
            auditLog.CreatedAt = DateTime.UtcNow;

            auditLog.SetCategory("AUDIT");

            auditLog.SetSeverity(
                isSoftDelete || entry.State == EntityState.Deleted
                    ? "WARNING"
                    : "INFO");

            auditLog.SetAuditData(
                changedFieldsJson,
                oldValuesJson,
                newValuesJson);

            if (changedFields.Count >= 2)
            {
                auditLog.MarkAiSummaryPending();
            }
            else
            {
                auditLog.SkipAiSummary();
            }

            context.Set<ChangeLog>().Add(auditLog);
        }

        return await base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }
}