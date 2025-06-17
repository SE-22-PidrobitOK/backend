using PidrobitOK.ModeratorService.Models.DTO;

namespace PidrobitOK.ModeratorService.Repositories
{
    public interface IModerationLogRepository
    {
        Task<IEnumerable<ModerationLogDto>> GetAllAsync();
        Task<ModerationLogDto?> GetByIdAsync(Guid id);
        Task AddAsync(ModerationLogDto dto);
        Task UpdateAsync(ModerationLogDto dto);
    }
}
