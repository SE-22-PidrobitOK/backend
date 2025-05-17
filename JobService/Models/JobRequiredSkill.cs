namespace JobService.Models
{
    public class JobRequiredSkill
    {
        public Guid Id { get; set; }

        public Guid JobId { get; set; }
        public Job? Job { get; set; }

        public Guid SkillId { get; set; }
        public Skill? Skill { get; set; }
    }
}