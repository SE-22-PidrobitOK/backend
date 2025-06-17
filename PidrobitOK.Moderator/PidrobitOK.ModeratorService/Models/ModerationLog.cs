using System.ComponentModel.DataAnnotations.Schema;

namespace PidrobitOK.ModeratorService.Models
{
    public class ModerationLog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Action { get; set; }
        public Guid? ModeratorId { get; set; }
        public Guid? ContentId { get; set; }
        public string? Reason { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
