namespace Application.DTOs.Logging
{
    public class BulkDeleteLogsByActionTypeDto
    {
        public string ActionType { get; set; } = null!;

        public int OlderThanDays { get; set; }
    }
}