namespace Application.DTOs.Logging
{
    public class ChangeLogDetailResponseDto
    {
        public int Id { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string? EntityName { get; set; } = string.Empty;
        public int? ReferenceId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? RawData { get; set; } = string.Empty;
        public string LogSource { get; set; } = string.Empty;
        public Guid CorrelationId { get; set; }
        public string? Category { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string? ChangedFields { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string AiSummaryStatus { get; set; } = string.Empty;
        public string? AiSummary { get; set; }
        public string? AiSummaryError { get; set; }
        public DateTime? AiSummaryGeneratedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}