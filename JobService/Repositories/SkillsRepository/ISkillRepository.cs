using JobService.Models;

namespace JobService.Repositories
{
    public interface ISkillRepository
    {
        public Task<SkillDto> Retrieve(Guid skillId);
        public Task<List<SkillDto>> Retrieve();
        Task<Skill> Insert(Skill skill);
        Task<Skill?> Update(Skill skill);
        Task<bool> Delete(Guid skillId);
        Task<bool> ExistsByName(string name);
    }
}
