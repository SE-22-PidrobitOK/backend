using System.ComponentModel.DataAnnotations.Schema;

namespace PidrobitOK.ModeratorService.Models
{
    public class Complaint
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Type { get; set; }
        public Guid TargetId { get; set; }
        public string Reason { get; set; }
        public bool IsResolved { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid ModeratorId { get; set; }
    }
}
