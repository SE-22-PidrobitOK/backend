using PidrobitOK.ModeratorService.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace PidrobitOK.ModeratorService.Models
{
    public class JobModeration
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public Guid ModeratorId { get; set; }
        public string? Reason { get; set; }
        public JobModerationStatus Status { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
