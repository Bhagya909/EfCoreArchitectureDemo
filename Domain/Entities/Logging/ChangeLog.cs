namespace Domain.Entities.Logging;

public class ChangeLog : BaseEntity
{
    public string ActionType { get; set; } = null!;

    public string? EntityName { get; set; }

    public int? ReferenceId { get; set; }

    public string? RawData { get; set; }

    public string? Summary { get; set; }
}