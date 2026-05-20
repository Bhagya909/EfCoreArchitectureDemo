using Domain.Enums;

namespace Domain.Entities.Logging;

public class ChangeLog : BaseEntity
{
    public string ActionType { get; set; } = null!;

    public string? EntityName { get; set; }

    public int? ReferenceId { get; set; }

    public string? RawData { get; set; }

    public string Description { get; private set; } = null!;

    public LogSource LogSource { get; private set; }

    public Guid CorrelationId { get; private set; }

    // Phase 10 additions

    public string? Category { get; private set; }

    public string Severity { get; private set; } = "INFO";

    public string? ChangedFields { get; private set; }

    public string? OldValues { get; private set; }

    public string? NewValues { get; private set; }

    public string? AiSummary { get; private set; }

    public AiSummaryStatus AiSummaryStatus
    { get; private set; }

    public string? AiSummaryError
    { get; private set; }

    public DateTime? AiSummaryGeneratedAt
    { get; private set; }

    private ChangeLog()
    {
    }

    public ChangeLog(
    string actionType,
    string? entityName,
    int? referenceId,
    string rawData,
    string description,
    LogSource logSource,
    Guid correlationId)
    {
        ActionType = actionType;
        EntityName = entityName;
        ReferenceId = referenceId;
        RawData = rawData;
        Description = description;
        LogSource = logSource;
        CorrelationId = correlationId;
        AiSummaryStatus =
            AiSummaryStatus.NotRequested;
    }
    public void SetCategory(string category)
    {
        Category = category;
    }

    public void SetSeverity(string severity)
    {
        Severity = severity;
    }

    public void SetAuditData(
        string changedFields,
        string oldValues,
        string newValues)
    {
        ChangedFields = changedFields;
        OldValues = oldValues;
        NewValues = newValues;
    }

    public void MarkAiSummaryPending()
    {
        AiSummaryStatus =
            AiSummaryStatus.Pending;

        AiSummaryError = null;
    }

    public void CompleteAiSummary(
    string aiSummary)
    {
        AiSummary = aiSummary;

        AiSummaryStatus =
            AiSummaryStatus.Completed;

        AiSummaryGeneratedAt =
            DateTime.UtcNow;

        AiSummaryError = null;
    }

    public void FailAiSummary(
    string error)
    {
        AiSummaryStatus =
            AiSummaryStatus.Failed;

        AiSummaryError = error;
    }
    public void SkipAiSummary()
    {
        AiSummaryStatus =
            AiSummaryStatus.Skipped;
    }
}