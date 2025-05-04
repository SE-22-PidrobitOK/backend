using JobService.Models;

namespace JobService.Repositories
{
    public interface ISkillRepository
    {
        public Task<SkillDto> Retrieve(Guid skillId);
        public Task<List<SkillDto>> Retrieve();
        public Task<bool> Insert(SkillDto skillDto);
    }
}
