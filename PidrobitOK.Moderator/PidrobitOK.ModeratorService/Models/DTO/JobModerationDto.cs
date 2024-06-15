using PidrobitOK.ModeratorService.Enums;

namespace PidrobitOK.ModeratorService.Models.DTO
{
    public class JobModerationDto
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public Guid ModeratorId { get; set; }
        public string? Reason { get; set; }
        public JobModerationStatus Status { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
