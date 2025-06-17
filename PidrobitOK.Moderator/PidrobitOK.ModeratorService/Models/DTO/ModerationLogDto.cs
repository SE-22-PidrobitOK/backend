namespace PidrobitOK.ModeratorService.Models.DTO
{
    public class ModerationLogDto
    {
        public Guid Id { get; set; }
        public string Action { get; set; }
        public Guid ModeratorId { get; set; }
        public Guid ContentId { get; set; }
        public string? Reason { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
