using JobService.Models;

namespace JobService.Repositories
{
    public interface ISkillRepository
    {
        public Task<SkillDto> Retrieve(Guid skillId);
        public Task<List<SkillDto>> Retrieve();
        public Task<bool> Insert(SkillDto skillDto);
        Task<bool> Update(SkillDto skillDto);       // додано
        Task<bool> Delete(Guid skillId);
    }
}
