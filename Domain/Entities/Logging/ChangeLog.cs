namespace Domain.Entities.Logging;

public class ChangeLog : BaseEntity
{
    public string ActionType { get; set; } = null!;

    public string? EntityName { get; set; }

    public int? ReferenceId { get; set; }

    public string? RawData { get; set; }

    public string? Summary { get; set; }

    private ChangeLog()
    {
    }

    public ChangeLog(
        string actionType,
        string? entityName,
        int? referenceId,
        string rawData,
        string? summary = null)
    {
        ActionType = actionType;
        EntityName = entityName;
        ReferenceId = referenceId;
        RawData = rawData;
        Summary = summary;
    }
}