namespace Application.DTOs.Logging
{
    public class ChangeLogSummaryDto
    {
        public int TotalLogs { get; set; }
        public int BusinessLogs { get; set; }
        public int AuditLogs { get; set; }
        public int PendingAiCount { get; set; }
        public int CompletedAiCount { get; set; }
        public int FailedAiCount { get; set; }
        public int SkippedAiCount { get; set; }
        public int NotRequestedAiCount { get; set; }
    }
}