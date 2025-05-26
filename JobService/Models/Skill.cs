namespace JobService.Models
{
    public class Skill
    {
        public Guid SkillId { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string Description { get; set; }

        public List<JobRequiredSkill> JobRequiredSkills { get; set; } = new();
    }
}
