using JobService.Models;

namespace JobService.Repositories.JobsRepository
{
    public interface IJobRepository
    {
        Task<Job?> Retrieve(Guid id);
        Task<List<Job>> Retrieve();
        Task<Job> Insert(Job job);
        Task<Job?> Update(Job job);
        Task<bool> Delete(Guid id);
    }
}
