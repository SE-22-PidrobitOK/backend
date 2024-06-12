namespace PidrobitOK.ModeratorService.Models.DTO
{
    public class ComplaintDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public Guid TargetId { get; set; }
        public string Reason { get; set; }
        public bool IsResolved { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid ModeratorId { get; set; }
    }
}
