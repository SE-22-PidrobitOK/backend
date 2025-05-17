using JobService.Models;

namespace JobService.Repositories.JobsRepository
{
    public interface IJobRepository
    {
        Task<JobDto> Retrieve(Guid id);
        Task<List<JobDto>> Retrieve();
        Task<bool> Insert(JobDto jobDto);
        Task<bool> Update(JobDto jobDto);
        Task<bool> Delete(Guid id);
    }
}
