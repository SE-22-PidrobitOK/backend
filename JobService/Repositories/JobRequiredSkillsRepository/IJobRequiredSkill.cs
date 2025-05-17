using JobService.Models;

namespace JobService.Repositories.JobRequiredSkillsRepository
{
    public interface IJobRequiredSkillRepository
    {
        Task<List<JobRequiredSkillDto>> Retrieve();
        Task<bool> Insert(JobRequiredSkillDto dto);
        Task<bool> Delete(Guid id);
    }
}