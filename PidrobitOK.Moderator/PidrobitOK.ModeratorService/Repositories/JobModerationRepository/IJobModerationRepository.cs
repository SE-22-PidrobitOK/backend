using PidrobitOK.ModeratorService.Models.DTO;

namespace PidrobitOK.ModeratorService.Repositories
{
    public interface IJobModerationRepository
    {
        Task<IEnumerable<JobModerationDto>> GetAllAsync();
        Task<JobModerationDto?> GetByIdAsync(Guid id);
        Task AddAsync(JobModerationDto dto);
        Task UpdateAsync(JobModerationDto dto);
    }
}
