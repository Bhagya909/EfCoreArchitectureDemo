namespace Application.DTOs.Logging
{
    public class ChangeLogResponseDto
    {
        public int Id { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string? EntityName { get; set; } = string.Empty;
        public int? ReferenceId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string LogSource { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string AiSummaryStatus { get; set; } = string.Empty;
        public string? AiSummary { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}